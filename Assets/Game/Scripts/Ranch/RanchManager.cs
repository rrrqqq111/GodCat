using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Abilities;
using NekogamiRanch.Abilities.Prey;
using NekogamiRanch.Animals;
using NekogamiRanch.Items;
using NekogamiRanch.Toys;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NekogamiRanch.Ranch
{
    public class RanchManager : MonoBehaviour
    {
        private const string AnimalDataRoot = "Assets/Game/Data/Animals";

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
        [SerializeField] private AnimalOfferRoller offerRoller;
        [SerializeField] private AnimalView animalViewPrefab;
        [SerializeField] private Sprite fallbackAnimalSprite;
        [SerializeField, Min(0)] private int randomStartingAnimalCount = 3;
        [SerializeField] private List<ItemData> startingItems = new List<ItemData>();
        [SerializeField] private List<ToyData> equippedToys = new List<ToyData>();

        private RanchGameState state;
        private RanchEconomyService economyService;
        private RanchAnimalService animalService;
        private RanchOfferService offerService;
        private RanchSettlementService settlementService;
        private RanchItemService itemService;
        private RanchToyService toyService;
        private MapCell selectedCell;
        private bool initialized;

        public event Action StateChanged;
        public event Action<PreyContext> OnPreyAttempt;
        public event Action<ProtectionResult> OnPreyProtected;
        public event Action<PreyResult> OnPreySuccess;
        public event Action<PreyResult> OnPreyFailed;
        public event Action<Animal, Animal> OnAnimalPreyed;
        public event Action<Animal> OnAnimalRemoved;
        public event Action<Animal> OnAnimalSold;
        public event Action<Animal, AnimalData> OnAnimalGrown;
        public event Action<Animal, AnimalData> OnAnimalTransformed;
        public event Action<AnimalCooldownReductionContext> OnAnimalCooldownReduced;

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
        }

        private void OnValidate()
        {
            RefreshOfferPoolFromConfiguredFamilies();
        }
#endif

        private void InitializeFromScene()
        {
            if (initialized)
            {
                return;
            }

            RefreshOfferPoolFromConfiguredFamilies();
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
            if (context?.Predator == null || context.TargetRule == null || animalService == null || ranchMap == null)
            {
                return CompletePreyResult(PreyResult.Failed(
                    context != null ? context.Predator : null,
                    Array.Empty<Animal>(),
                    Array.Empty<ProtectionResult>(),
                    "InvalidPreyContext"));
            }

            OnPreyAttempt?.Invoke(context);
            var candidateTargets = PreyTargetResolver.Resolve(context, ranchMap);
            var protectionResults = new List<ProtectionResult>();
            if (candidateTargets.Count == 0)
            {
                return CompletePreyResult(PreyResult.Failed(
                    context.Predator,
                    candidateTargets,
                    protectionResults,
                    "NoCandidateTargets"));
            }

            var removedTargets = new List<Animal>();
            foreach (var target in candidateTargets)
            {
                var protectionResult = ResolveProtection(context, target);
                if (protectionResult.Success)
                {
                    protectionResults.Add(protectionResult);
                    OnPreyProtected?.Invoke(protectionResult);
                    continue;
                }

                OnAnimalPreyed?.Invoke(context.Predator, target);
                settlementService?.ResolveAnimalPreyedAbilities(context.Predator, target, ranchMap);
                if (!animalService.AnimalRemoved(target))
                {
                    continue;
                }

                removedTargets.Add(target);
            }

            if (removedTargets.Count > 0)
            {
                if (!RefreshSelectionAfterAnimalRemoval())
                {
                    NotifyStateChanged();
                }

                return CompletePreyResult(PreyResult.Succeeded(
                    context.Predator,
                    candidateTargets,
                    protectionResults,
                    removedTargets));
            }

            var failureReason = protectionResults.Count == candidateTargets.Count ? "AllTargetsProtected" : "NoTargetRemoved";
            return CompletePreyResult(PreyResult.Failed(
                context.Predator,
                candidateTargets,
                protectionResults,
                failureReason));
        }

        public bool RemoveAnimal(Animal animal)
        {
            if (animalService == null || animal == null)
            {
                return false;
            }

            settlementService?.ResolveAnimalRemovedAbility(animal);
            OnAnimalRemoved?.Invoke(animal);
            var removed = animalService.AnimalRemoved(animal);

            if (selectedCell != null && selectedCell.Animal == null)
            {
                SelectCell(null);
            }

            return removed;
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

            OnAnimalSold?.Invoke(animal);
            if (!animalService.AnimalRemoved(animal))
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
            OnAnimalTransformed?.Invoke(oldAnimal, newAnimalData);
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

            OnAnimalGrown?.Invoke(youngAnimal, grownAnimalData);
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

            var removedAnimal = cell.Animal;
            if (removedAnimal != null)
            {
                settlementService?.ResolveAnimalRemovedAbility(removedAnimal);
                OnAnimalRemoved?.Invoke(removedAnimal);
                if (!animalService.AnimalRemoved(removedAnimal))
                {
                    return false;
                }
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

            settlementService?.ResolveAnimalRemovedAbility(removedAnimal);
            OnAnimalRemoved?.Invoke(removedAnimal);
            var removed = animalService.AnimalRemovedAt(coords);
            if (!removed)
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

            if (!animalService.AnimalRemovedFromCell(cell))
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

            OnAnimalCooldownReduced?.Invoke(context);
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
            toyService = new RanchToyService(this, economyService, equippedToys);
        }

        private void RefreshOfferPoolFromConfiguredFamilies()
        {
#if UNITY_EDITOR
            if (!autoPopulateOfferPoolByFamily)
            {
                return;
            }

            offerPool = LoadAnimalsFromConfiguredFamilies();
#endif
        }

#if UNITY_EDITOR
        private List<AnimalData> LoadAnimalsFromConfiguredFamilies()
        {
            var familyFilters = (offerPoolFamilies ?? new List<string>())
                .Where(family => !string.IsNullOrWhiteSpace(family))
                .Select(family => family.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (familyFilters.Count == 0)
            {
                return new List<AnimalData>();
            }

            var familySet = new HashSet<string>(familyFilters, StringComparer.OrdinalIgnoreCase);
            return AssetDatabase.FindAssets("t:AnimalData", new[] { AnimalDataRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AnimalData>)
                .Where(data => data != null && familySet.Contains(data.Family))
                .OrderBy(data => GetFamilySortIndex(familyFilters, data.Family))
                .ThenBy(data => data.Rarity)
                .ThenBy(data => data.DisplayName)
                .ThenBy(data => data.Id)
                .ToList();
        }

        private static int GetFamilySortIndex(IReadOnlyList<string> familyFilters, string family)
        {
            for (var i = 0; i < familyFilters.Count; i++)
            {
                if (string.Equals(familyFilters[i], family, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return int.MaxValue;
        }
#endif

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
            toyService?.Trigger(ToyTriggerType.DayStart, Day);
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

        private PreyResult CompletePreyResult(PreyResult result)
        {
            if (result.Success)
            {
                OnPreySuccess?.Invoke(result);
            }
            else
            {
                OnPreyFailed?.Invoke(result);
            }

            return result;
        }

        private ProtectionResult ResolveProtection(PreyContext context, Animal target)
        {
            if (context == null || target == null || context.ProtectionRules == null || context.ProtectionRules.Count == 0)
            {
                return ProtectionResult.Unprotected(target);
            }

            foreach (var rule in context.ProtectionRules)
            {
                var candidateProtectors = ResolveCandidateProtectors(rule, target);
                var protectionContext = new ProtectionContext(context.Predator, target, candidateProtectors, context.SourceAbilityId);
                foreach (var protector in protectionContext.CandidateProtectors)
                {
                    if (!rule.CanProtect(protector, context.Predator, target))
                    {
                        continue;
                    }

                    var reason = string.IsNullOrWhiteSpace(rule.Reason) ? "Protected" : rule.Reason;
                    return ProtectionResult.Protected(protector, target, rule, reason);
                }
            }

            return ProtectionResult.Unprotected(target);
        }

        private IReadOnlyList<Animal> ResolveCandidateProtectors(ProtectionRule rule, Animal target)
        {
            var protectors = new List<Animal>();
            if (rule == null || target == null || ranchMap == null)
            {
                return protectors;
            }

            if (rule.Protector != null)
            {
                if (IsAnimalOnMap(rule.Protector) && ProtectionScopeContainsTarget(rule.Scope, rule.Protector, target))
                {
                    protectors.Add(rule.Protector);
                }

                return protectors;
            }

            foreach (var cell in ranchMap.GetCellsInScanOrder())
            {
                var protector = cell.Animal;
                if (protector == null || !rule.MatchesProtector(protector))
                {
                    continue;
                }

                if (ProtectionScopeContainsTarget(rule.Scope, protector, target))
                {
                    protectors.Add(protector);
                }
            }

            return protectors;
        }

        private bool ProtectionScopeContainsTarget(string scope, Animal protector, Animal target)
        {
            if (protector == null || target == null || ranchMap == null)
            {
                return false;
            }

            switch (NormalizeRuleText(scope))
            {
                case "self":
                    return protector == target;
                case "adjacent":
                    return ranchMap.GetNeighbors(protector.Coords).Any(cell => cell.Animal == target);
                case "left":
                    return target.Coords == protector.Coords + Vector2Int.left;
                case "right":
                    return target.Coords == protector.Coords + Vector2Int.right;
                case "up":
                case "upper":
                case "upperadjacent":
                    return ranchMap.GetUpperNeighbors(protector.Coords).Any(cell => cell.Animal == target);
                case "down":
                case "lower":
                case "loweradjacent":
                    return ranchMap.GetLowerNeighbors(protector.Coords).Any(cell => cell.Animal == target);
                case "row":
                    return protector.Coords.y == target.Coords.y;
                case "all":
                case "global":
                case "field":
                    return true;
                default:
                    return false;
            }
        }

        private bool IsAnimalOnMap(Animal animal)
        {
            return animal != null &&
                ranchMap != null &&
                ranchMap.TryGetCell(animal.Coords, out var cell) &&
                cell.Animal == animal;
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

        private static string NormalizeRuleText(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
        }
    }
}
