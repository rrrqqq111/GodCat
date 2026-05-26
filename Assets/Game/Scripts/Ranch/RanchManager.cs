using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Abilities;
using NekogamiRanch.Abilities.Prey;
using NekogamiRanch.Animals;
using NekogamiRanch.Items;
using NekogamiRanch.Toys;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchManager : MonoBehaviour
    {
        private const string AnimalDataRoot = "Assets/Game/Data/Animals";
        private const string ItemDataRoot = "Assets/Game/Data/Items";

        [SerializeField] private int mapWidth = 4;
        [SerializeField] private int mapHeight = 5;
        [SerializeField] private int day = 1;
        [SerializeField] private int money;
        [SerializeField, Min(0)] private int cans = 5;
        [SerializeField, Min(0)] private int removeAnimalCansCost = 1;
        [SerializeField] private RanchMap ranchMap;
        [SerializeField] private bool autoPopulateOfferPoolByFamily = true;
        [SerializeField] private List<string> offerPoolFamilies = new List<string> { "Hoofed", "Carnivora" };
        [SerializeField, HideInInspector] private List<AnimalData> offerPool = new List<AnimalData>();
        [SerializeField, HideInInspector] private List<AnimalData> abilitySpawnPool = new List<AnimalData>();
        [SerializeField] private AnimalOfferRoller offerRoller;
        [SerializeField] private AnimalView animalViewPrefab;
        [SerializeField] private Sprite fallbackAnimalSprite;
        [SerializeField, Min(0)] private int randomStartingAnimalCount = 3;
        [SerializeField] private List<ItemData> startingItems = new List<ItemData>();
        [SerializeField, HideInInspector] private List<ItemData> itemRewardPool = new List<ItemData>();
        [SerializeField] private List<ToyData> equippedToys = new List<ToyData>();

        private readonly RanchEventHub eventHub = new RanchEventHub();
        private RanchGameState state;
        private RanchEconomyService economyService;
        private RanchAnimalService animalService;
        private RanchAnimalLifecycleService animalLifecycleService;
        private RanchAnimalSpawnService animalSpawnService;
        private RanchProtectionService protectionService;
        private RanchPreyService preyService;
        private RanchTurnService turnService;
        private RanchOfferService offerService;
        private RanchSettlementService settlementService;
        private RanchItemService itemService;
        private RanchRewardService rewardService;
        private RanchToyService toyService;
        private MapCell selectedCell;
        private bool initialized;

        public event Action StateChanged
        {
            add => eventHub.StateChanged += value;
            remove => eventHub.StateChanged -= value;
        }

        public event Action<PreyContext> OnPreyAttempt
        {
            add => eventHub.PreyAttempted += value;
            remove => eventHub.PreyAttempted -= value;
        }

        public event Action<ProtectionResult> OnPreyProtected
        {
            add => eventHub.PreyProtected += value;
            remove => eventHub.PreyProtected -= value;
        }

        public event Action<PreyResult> OnPreySuccess
        {
            add => eventHub.PreySucceeded += value;
            remove => eventHub.PreySucceeded -= value;
        }

        public event Action<PreyResult> OnPreyFailed
        {
            add => eventHub.PreyFailed += value;
            remove => eventHub.PreyFailed -= value;
        }

        public event Action<Animal, Animal> OnAnimalPreyed
        {
            add => eventHub.AnimalPreyed += value;
            remove => eventHub.AnimalPreyed -= value;
        }

        public event Action<Animal> OnAnimalRemoved
        {
            add => eventHub.AnimalRemoved += value;
            remove => eventHub.AnimalRemoved -= value;
        }

        public event Action<Animal> OnAnimalSold
        {
            add => eventHub.AnimalSold += value;
            remove => eventHub.AnimalSold -= value;
        }

        public event Action<Animal, AnimalData> OnAnimalGrown
        {
            add => eventHub.AnimalGrown += value;
            remove => eventHub.AnimalGrown -= value;
        }

        public event Action<Animal, AnimalData> OnAnimalTransformed
        {
            add => eventHub.AnimalTransformed += value;
            remove => eventHub.AnimalTransformed -= value;
        }

        public event Action<AnimalCooldownReductionContext> OnAnimalCooldownReduced
        {
            add => eventHub.AnimalCooldownReduced += value;
            remove => eventHub.AnimalCooldownReduced -= value;
        }

        public int Day => state != null ? state.Day : day;
        public int Money => state != null ? state.Money : money;
        public int Cans => state != null ? state.Cans : cans;
        public int RemoveAnimalCansCost => removeAnimalCansCost;
        public RanchMap Map => ranchMap;
        public MapCell SelectedCell => selectedCell;
        public bool IsWaitingForOfferSelection => state != null && state.IsWaitingForOfferSelection;
        public bool IsWaitingToEnterNextDay => state != null && state.IsWaitingToEnterNextDay;
        public bool IsTestMode => state != null && state.IsTestMode;
        public bool RandomizeAnimalPositionsInTestMode => state == null || state.RandomizeAnimalPositionsInTestMode;
        public IReadOnlyList<AnimalData> CurrentOffers => offerService != null ? offerService.CurrentOffers : Array.Empty<AnimalData>();
        public IReadOnlyList<ItemRuntimeState> CurrentItems => itemService != null ? itemService.Items : Array.Empty<ItemRuntimeState>();
        public IReadOnlyList<string> CurrentItemIds => itemService != null ? itemService.ItemIds : Array.Empty<string>();
        public IReadOnlyList<ToyData> CurrentToys => toyService != null ? toyService.EquippedToys : Array.Empty<ToyData>();
        public IReadOnlyList<string> CurrentToyIds => toyService != null ? toyService.EquippedToyIds : Array.Empty<string>();
        public string LastSettlementReport => settlementService != null ? settlementService.LastReport : "暂无结算";

        private void Start()
        {
            InitializeFromScene();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            RefreshOfferPoolFromConfiguredFamilies();
            RefreshAbilitySpawnPool();
            RefreshItemRewardPool();
        }

        private void OnValidate()
        {
            RefreshOfferPoolFromConfiguredFamilies();
            RefreshAbilitySpawnPool();
            RefreshItemRewardPool();
        }
#endif

        private void InitializeFromScene()
        {
            if (initialized)
            {
                return;
            }

            RefreshOfferPoolFromConfiguredFamilies();
            RefreshAbilitySpawnPool();
            RefreshItemRewardPool();
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

            RefreshOfferPoolFromConfiguredFamilies();
            RefreshAbilitySpawnPool();
            RefreshItemRewardPool();
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
            TriggerToys(ToyTriggerType.RunStart);
            SeedAnimals(startingAnimals);
            CreateTurnService();
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
            turnService?.NextDay();
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

        public bool TryAddRandomItem()
        {
            if (rewardService == null || !rewardService.TryGrantRandomItem(out _))
            {
                return false;
            }

            NotifyStateChanged();
            return true;
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

        public bool TryGetToyById(string toyId, out ToyData toy)
        {
            toy = null;
            return toyService != null && toyService.TryGetToyById(toyId, out toy);
        }

        public Sprite GetToyIconById(string toyId)
        {
            return TryGetToyById(toyId, out var toy) ? toy.Icon : null;
        }

        public string GetToyDescriptionById(string toyId)
        {
            return TryGetToyById(toyId, out var toy) ? toy.Description : string.Empty;
        }

        public void TriggerToys(ToyTriggerType triggerType)
        {
            toyService?.Trigger(triggerType, Day);
        }

        public bool RegisterToy(ToyData toyData)
        {
            var registered = toyService != null && toyService.Register(toyData);
            if (registered)
            {
                NotifyStateChanged();
            }

            return registered;
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

        public PreyResult TryPrey(PreyContext context)
        {
            PreyResult result;
            if (preyService != null)
            {
                result = preyService.TryPrey(context);
            }
            else
            {
                result = PreyResult.Failed(
                    context != null ? context.Predator : null,
                    Array.Empty<Animal>(),
                    Array.Empty<ProtectionResult>(),
                    "PreyServiceUnavailable");
                eventHub.NotifyPreyCompleted(result);
            }

            if (result.Success)
            {
                if (!RefreshSelectionAfterAnimalRemoval())
                {
                    NotifyStateChanged();
                }
            }
            else if (result.ProtectedTargets.Count > 0)
            {
                NotifyStateChanged();
            }

            return result;
        }

        public bool RemoveAnimal(Animal animal)
        {
            if (animalService == null || animal == null)
            {
                return false;
            }

            var removed = animalLifecycleService != null && animalLifecycleService.TryRemove(animal);
            if (!removed)
            {
                return false;
            }

            if (selectedCell != null && selectedCell.Animal == null)
            {
                SelectCell(null);
            }
            else
            {
                NotifyStateChanged();
            }

            return true;
        }

        public bool TryRemoveAnimalWithCans(Animal animal)
        {
            if (animalService == null || animal == null || !IsAnimalOnMap(animal))
            {
                return false;
            }

            if (!TrySpendCans(removeAnimalCansCost))
            {
                return false;
            }

            return RemoveAnimal(animal);
        }

        public bool SellAnimal(Animal animal)
        {
            if (animalService == null || animal == null)
            {
                return false;
            }

            if (animalLifecycleService == null ||
                !animalLifecycleService.TryRemove(animal, AnimalRemovalReason.Sold))
            {
                return false;
            }

            if (selectedCell != null && selectedCell.Animal == null)
            {
                SelectCell(null);
            }
            else
            {
                NotifyStateChanged();
            }

            return true;
        }

        public bool TransformAnimal(Animal oldAnimal, AnimalData newAnimalData)
        {
            if (animalService == null || oldAnimal == null || newAnimalData == null)
            {
                return false;
            }

            var wasSelectedAnimal = selectedCell != null && selectedCell.Animal == oldAnimal;
            eventHub.NotifyAnimalTransformed(oldAnimal, newAnimalData);
            var replaced = animalService.ReplaceAnimal(oldAnimal, newAnimalData);
            if (replaced && wasSelectedAnimal)
            {
                SelectCell(null);
            }

            return replaced;
        }

        public bool GrowAnimal(Animal youngAnimal, AnimalData grownAnimalData)
        {
            if (animalService == null || youngAnimal == null || grownAnimalData == null)
            {
                return false;
            }

            eventHub.NotifyAnimalGrown(youngAnimal, grownAnimalData);
            var grown = animalService.ReplaceAnimal(youngAnimal, grownAnimalData);
            if (!grown)
            {
                return false;
            }

            if (selectedCell != null && selectedCell.Animal == youngAnimal)
            {
                SelectCell(null);
            }
            else
            {
                NotifyStateChanged();
            }

            return true;
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

        public bool TryAddAnimalToRandomEmptyCell(AnimalData animalData)
        {
            if (animalService == null || animalData == null)
            {
                return false;
            }

            var added = animalService.TryAddAnimalToRandomEmptyCell(animalData);
            if (added)
            {
                NotifyStateChanged();
            }

            return added;
        }

        public bool TryAddRandomAnimalFromFamily(string family, int baseMoneyBonus, out Animal addedAnimal)
        {
            addedAnimal = null;
            if (animalSpawnService == null ||
                !animalSpawnService.TrySpawnRandomFromFamily(family, baseMoneyBonus, out addedAnimal))
            {
                return false;
            }

            NotifyStateChanged();
            return true;
        }

        public bool TryClearAnimalAt(Vector2Int coords)
        {
            if (animalService == null || ranchMap == null || !ranchMap.TryGetCell(coords, out var cell))
            {
                return false;
            }

            var removedAnimal = cell.Animal;
            if (removedAnimal == null)
            {
                return false;
            }

            if (animalLifecycleService == null || !animalLifecycleService.TryRemove(removedAnimal))
            {
                return false;
            }

            SelectCell(cell);
            NotifyStateChanged();
            return true;
        }

        public bool DeleteAnimalAtForTest(Vector2Int coords)
        {
            if (animalService == null || ranchMap == null || !ranchMap.TryGetCell(coords, out var cell))
            {
                return false;
            }

            var removedAnimal = cell.Animal;
            if (removedAnimal == null || !animalService.AnimalRemovedFromCell(cell))
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

        public void NotifyAnimalCooldownReduced(AnimalCooldownReductionContext context)
        {
            if (context.Target == null || context.Amount <= 0)
            {
                return;
            }

            eventHub.NotifyAnimalCooldownReduced(context);
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
            animalLifecycleService = new RanchAnimalLifecycleService(
                animalService,
                settlementService,
                ranchMap,
                eventHub);
            protectionService = new RanchProtectionService(ranchMap, economyService.AddMoney);
            preyService = new RanchPreyService(ranchMap, animalLifecycleService, protectionService, eventHub);
            animalSpawnService = new RanchAnimalSpawnService(animalService, abilitySpawnPool);
            var configuredRewardPool = itemRewardPool != null && itemRewardPool.Count > 0 ? itemRewardPool : startingItems;
            rewardService = new RanchRewardService(itemService, configuredRewardPool);
            toyService = new RanchToyService(this, economyService, equippedToys);
        }

        private void RefreshOfferPoolFromConfiguredFamilies()
        {
#if UNITY_EDITOR
            if (!autoPopulateOfferPoolByFamily)
            {
                return;
            }

            offerPool = RanchContentCatalog.LoadOfferAnimals(AnimalDataRoot, offerPoolFamilies);
#endif
        }

        private void RefreshItemRewardPool()
        {
#if UNITY_EDITOR
            itemRewardPool = RanchContentCatalog.LoadItems(ItemDataRoot);
#endif
        }

        private void RefreshAbilitySpawnPool()
        {
#if UNITY_EDITOR
            abilitySpawnPool = RanchContentCatalog.LoadAnimals(AnimalDataRoot);
#endif
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

        private void NotifyStateChanged()
        {
            eventHub.NotifyStateChanged();
        }

        private void ResolveMovedAbility(Animal animal, Vector2Int previousCoords)
        {
            settlementService?.ResolveMovedAbility(animal);
            settlementService?.ResolveAdjacentAnimalMovedAbilities(animal, previousCoords, ranchMap);
        }

        private void CreateTurnService()
        {
            turnService = new RanchTurnService(
                state,
                ranchMap,
                animalService,
                offerService,
                settlementService,
                itemService,
                toyService,
                NotifyStateChanged);
        }

        private bool IsAnimalOnMap(Animal animal)
        {
            return animalService != null && animalService.IsAnimalOnMap(animal);
        }

        private bool RefreshSelectionAfterAnimalRemoval()
        {
            if (selectedCell != null && selectedCell.Animal == null)
            {
                SelectCell(null);
                return true;
            }

            return false;
        }

    }
}
