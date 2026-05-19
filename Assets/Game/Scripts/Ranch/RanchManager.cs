using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Abilities;
using NekogamiRanch.Animals;
using NekogamiRanch.Items;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchManager : MonoBehaviour
    {
        [SerializeField] private int mapWidth = 4;
        [SerializeField] private int mapHeight = 5;
        [SerializeField] private int day = 1;
        [SerializeField] private int money;
        [SerializeField, Min(0)] private int cans = 5;
        [SerializeField] private RanchMap ranchMap;
        [SerializeField] private List<AnimalData> offerPool = new List<AnimalData>();
        [SerializeField] private AnimalOfferRoller offerRoller;
        [SerializeField] private AnimalView animalViewPrefab;
        [SerializeField] private Sprite fallbackAnimalSprite;
        [SerializeField, Min(0)] private int randomStartingAnimalCount = 3;
        [SerializeField] private List<ItemData> startingItems = new List<ItemData>();

        private RanchGameState state;
        private RanchEconomyService economyService;
        private RanchAnimalService animalService;
        private RanchOfferService offerService;
        private RanchSettlementService settlementService;
        private RanchItemService itemService;
        private MapCell selectedCell;
        private bool initialized;

        public event Action StateChanged;

        public int Day => state != null ? state.Day : day;
        public int Money => state != null ? state.Money : money;
        public int Cans => state != null ? state.Cans : cans;
        public RanchMap Map => ranchMap;
        public MapCell SelectedCell => selectedCell;
        public bool IsWaitingForOfferSelection => state != null && state.IsWaitingForOfferSelection;
        public bool IsWaitingToEnterNextDay => state != null && state.IsWaitingToEnterNextDay;
        public bool IsTestMode => state != null && state.IsTestMode;
        public bool RandomizeAnimalPositionsInTestMode => state == null || state.RandomizeAnimalPositionsInTestMode;
        public IReadOnlyList<AnimalData> CurrentOffers => offerService != null ? offerService.CurrentOffers : Array.Empty<AnimalData>();
        public IReadOnlyList<ItemRuntimeState> CurrentItems => itemService != null ? itemService.Items : Array.Empty<ItemRuntimeState>();
        public IReadOnlyList<string> CurrentItemIds => itemService != null ? itemService.ItemIds : Array.Empty<string>();
        public string LastSettlementReport => settlementService != null ? settlementService.LastReport : "暂无结算";

        private void Start()
        {
            InitializeFromScene();
        }

        private void InitializeFromScene()
        {
            if (initialized)
            {
                return;
            }

            ranchMap = RanchSceneBinder.ResolveMap(ranchMap);

            if (ranchMap == null)
            {
                Debug.LogError("[RanchManager] RanchMap is missing. Add a RanchMap object to the scene and assign it.");
                return;
            }

            var startingAnimals = RollRandomStartingAnimals(randomStartingAnimalCount);
            Initialize(ranchMap, startingAnimals, null, GetFallbackAnimalSprite(), RanchSceneBinder.FindSceneTileRenderers());
        }

        public void Initialize(RanchMap map, IReadOnlyList<AnimalData> startingAnimals, Sprite tileSprite, Sprite animalSprite, IReadOnlyList<SpriteRenderer> sceneTiles = null)
        {
            if (initialized)
            {
                return;
            }

            ranchMap = map;
            if (ranchMap == null)
            {
                Debug.LogError("[RanchManager] Cannot initialize without a RanchMap.");
                return;
            }

            if (offerRoller == null)
            {
                offerRoller = GetComponent<AnimalOfferRoller>();
            }

            ranchMap.Initialize(this, mapWidth, mapHeight, tileSprite, animalSprite != null ? animalSprite : GetFallbackAnimalSprite(), sceneTiles ?? RanchSceneBinder.FindSceneTileRenderers(), animalViewPrefab);
            CreateServices();
            SeedAnimals(startingAnimals);
            SelectCell(null);
            initialized = true;
            NotifyStateChanged();
        }

        public void SelectCell(MapCell cell)
        {
            if (selectedCell != null)
            {
                selectedCell.SetSelected(false);
            }

            selectedCell = cell;
            if (selectedCell != null)
            {
                selectedCell.SetSelected(true);
            }

            NotifyStateChanged();
        }

        public void NextDay()
        {
            if (state != null && state.Phase == RanchPhase.DayTransition)
            {
                EnterNextDay();
                return;
            }

            if (state != null && state.Phase == RanchPhase.OfferSelection)
            {
                return;
            }

            ResolveDailySettlement();
            if (IsTestMode)
            {
                state?.SetPhase(RanchPhase.DayTransition);
                NotifyStateChanged();
                return;
            }

            RollOffers(3);
            state?.SetPhase(CurrentOffers.Count > 0 ? RanchPhase.OfferSelection : RanchPhase.DayTransition);
            NotifyStateChanged();
        }

        public void AddMoney(int amount)
        {
            if (settlementService != null)
            {
                settlementService.AddMoney(amount);
                return;
            }

            if (economyService != null)
            {
                economyService.AddMoney(amount);
            }
            else
            {
                money += amount;
            }
        }

        public void AddCans(int amount)
        {
            economyService?.AddCans(amount);
            NotifyStateChanged();
        }

        public bool TrySpendCans(int amount)
        {
            var spent = amount <= 0 || (economyService != null && economyService.TrySpendCans(amount));
            NotifyStateChanged();
            return spent;
        }

        public bool AddItem(ItemData itemData, int count = 1)
        {
            var added = itemService != null && itemService.AddItem(itemData, count);
            if (added)
            {
                NotifyStateChanged();
            }

            return added;
        }

        public bool TryGetItemById(string itemId, out ItemRuntimeState item)
        {
            item = null;
            return itemService != null && itemService.TryGetItemById(itemId, out item);
        }

        public Sprite GetItemIconById(string itemId)
        {
            return TryGetItemById(itemId, out var item) && item.Data != null ? item.Data.Icon : null;
        }

        public string GetItemDescriptionById(string itemId)
        {
            return TryGetItemById(itemId, out var item) && item.Data != null ? item.Data.Description : string.Empty;
        }

        public void AddExtraMoney(Animal source, int amount)
        {
            economyService?.AddExtraMoney(source, amount);
        }

        public bool TryMoveAnimal(Animal animal, Vector2Int targetCoords)
        {
            return animalService != null && animalService.TryMoveAnimal(animal, targetCoords);
        }

        public bool TrySwapAnimals(Animal first, Animal second)
        {
            return animalService != null && animalService.TrySwapAnimals(first, second);
        }

        public bool RemoveAnimal(Animal animal)
        {
            if (animalService == null)
            {
                return false;
            }

            var removed = animalService.RemoveAnimal(animal);
            if (selectedCell != null && selectedCell.Animal == null)
            {
                SelectCell(null);
            }

            return removed;
        }

        public bool ReplaceAnimal(Animal oldAnimal, AnimalData newAnimalData)
        {
            if (animalService == null)
            {
                return false;
            }

            var wasSelectedAnimal = selectedCell != null && selectedCell.Animal == oldAnimal;
            var replaced = animalService.ReplaceAnimal(oldAnimal, newAnimalData);
            if (replaced && wasSelectedAnimal)
            {
                SelectCell(null);
            }

            return replaced;
        }

        public bool TrySetAnimalAt(Vector2Int coords, AnimalData animalData)
        {
            if (animalService == null || ranchMap == null || !ranchMap.TryGetCell(coords, out var cell))
            {
                return false;
            }

            if (!animalService.TrySetAnimalAt(coords, animalData))
            {
                return false;
            }

            SelectCell(cell);
            NotifyStateChanged();
            return true;
        }

        public bool TryClearAnimalAt(Vector2Int coords)
        {
            if (animalService == null || ranchMap == null || !ranchMap.TryGetCell(coords, out var cell))
            {
                return false;
            }

            var removed = animalService.TryClearAnimalAt(coords);
            if (!removed)
            {
                return false;
            }

            SelectCell(cell);
            NotifyStateChanged();
            return true;
        }

        public void ClearAllAnimals()
        {
            animalService?.ClearAllAnimals();
            SelectCell(null);
            NotifyStateChanged();
        }

        public void EnterTestMode()
        {
            state?.EnterTestMode();
            offerService?.Clear();
            settlementService?.SetLastReport("测试模式：请点击地块并添加动物");
            ClearAllAnimals();
        }

        public void ExitTestMode()
        {
            state?.ExitTestMode();
            offerService?.Clear();
            NotifyStateChanged();
        }

        public void SetRandomizeAnimalPositionsInTestMode(bool enabled)
        {
            state?.SetRandomizeAnimalPositionsInTestMode(enabled);
            NotifyStateChanged();
        }

        public bool SelectOffer(int index)
        {
            if (!IsWaitingForOfferSelection || offerService == null || animalService == null)
            {
                return false;
            }

            var added = offerService.SelectOffer(index, animalService);
            state?.SetPhase(RanchPhase.DayTransition);
            NotifyStateChanged();
            return added;
        }

        public int CountAnimalsById(string animalId)
        {
            return animalService != null ? animalService.CountAnimalsById(animalId) : 0;
        }

        public bool HasAnimalById(string animalId)
        {
            return animalService != null && animalService.HasAnimalById(animalId);
        }

        public bool TryTriggerAnimalAbility(Animal animal)
        {
            return TryTriggerAnimalAbilityWithResult(animal).Success;
        }

        public AbilityExecutionResult TryTriggerAnimalAbilityWithResult(Animal animal)
        {
            return settlementService != null
                ? settlementService.TryTriggerAnimalAbility(animal)
                : AbilityExecutionResult.Failed();
        }

        public string GetSelectedCellText()
        {
            return RanchTextFormatter.GetSelectedCellText(selectedCell);
        }

        private void CreateServices()
        {
            state = new RanchGameState(day, money, cans);
            economyService = new RanchEconomyService(state);
            animalService = new RanchAnimalService(ranchMap, ResolveMovedAbility);
            offerService = new RanchOfferService(offerRoller, offerPool);
            settlementService = new RanchSettlementService(this, animalService, economyService);
            itemService = new RanchItemService(this, economyService, startingItems);
        }

        private void SeedAnimals(IReadOnlyList<AnimalData> startingAnimals)
        {
            if (startingAnimals != null && startingAnimals.Count > 0 && offerPool.Count == 0)
            {
                offerPool = startingAnimals.Where(data => data != null).Distinct().ToList();
                offerService = new RanchOfferService(offerRoller, offerPool);
            }

            animalService?.SeedAnimals(startingAnimals);
        }

        private IReadOnlyList<AnimalData> RollRandomStartingAnimals(int count)
        {
            if (count <= 0 || offerPool == null || offerPool.Count == 0)
            {
                return Array.Empty<AnimalData>();
            }

            var validPool = offerPool.Where(data => data != null).ToList();
            if (validPool.Count == 0)
            {
                return Array.Empty<AnimalData>();
            }

            var results = new List<AnimalData>();
            for (var i = 0; i < count; i++)
            {
                results.Add(validPool[UnityEngine.Random.Range(0, validPool.Count)]);
            }

            return results;
        }

        private Sprite GetFallbackAnimalSprite()
        {
            if (fallbackAnimalSprite == null)
            {
                fallbackAnimalSprite = RanchSceneBinder.GetFallbackAnimalSprite(null);
            }

            return fallbackAnimalSprite;
        }

        private void EnterNextDay()
        {
            if (!IsTestMode || RandomizeAnimalPositionsInTestMode)
            {
                animalService?.RandomizeAnimalPositions();
            }

            state?.AddDay();
            state?.SetPhase(IsTestMode ? RanchPhase.TestMode : RanchPhase.Playing);
            itemService?.Trigger(ItemTriggerType.DayStart, Day);
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            StateChanged?.Invoke();
        }

        private void ResolveDailySettlement()
        {
            settlementService?.ResolveDailySettlement(ranchMap);
        }

        private void ResolveMovedAbility(Animal animal)
        {
            settlementService?.ResolveMovedAbility(animal);
        }

        private void RollOffers(int count)
        {
            offerService?.Roll(Day, count);
        }
    }
}
