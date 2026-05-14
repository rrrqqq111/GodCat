using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NekogamiRanch.Abilities;
using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.Ranch
{
    public class RanchManager : MonoBehaviour
    {
        [SerializeField] private int mapWidth = 4;
        [SerializeField] private int mapHeight = 5;
        [SerializeField] private int day = 1;
        [SerializeField] private int money;
        [SerializeField] private List<AnimalData> offerPool = new List<AnimalData>();
        [SerializeField] private AnimalOfferRoller offerRoller;

        private readonly List<Animal> animals = new List<Animal>();
        private readonly List<AnimalData> currentOffers = new List<AnimalData>();
        private readonly StringBuilder settlementReportBuilder = new StringBuilder();
        private readonly List<SettlementAnimalReport> settlementAnimalReports = new List<SettlementAnimalReport>();
        private readonly Dictionary<Animal, SettlementAnimalReport> settlementReportByAnimal = new Dictionary<Animal, SettlementAnimalReport>();
        private RanchMap ranchMap;
        private MapCell selectedCell;
        private bool waitingForOfferSelection;
        private bool waitingToEnterNextDay;
        private bool testMode;
        private string lastSettlementReport = "\u6682\u65e0\u7ed3\u7b97";
        private Animal activeExtraMoneyOwner;

        public event Action StateChanged;

        public int Day => day;
        public int Money => money;
        public RanchMap Map => ranchMap;
        public MapCell SelectedCell => selectedCell;
        public bool IsWaitingForOfferSelection => waitingForOfferSelection;
        public bool IsWaitingToEnterNextDay => waitingToEnterNextDay;
        public bool IsTestMode => testMode;
        public IReadOnlyList<AnimalData> CurrentOffers => currentOffers;
        public string LastSettlementReport => lastSettlementReport;

        public void Initialize(RanchMap map, IReadOnlyList<AnimalData> startingAnimals, Sprite tileSprite, Sprite animalSprite, IReadOnlyList<SpriteRenderer> sceneTiles = null)
        {
            ranchMap = map;
            if (offerRoller == null)
            {
                offerRoller = GetComponent<AnimalOfferRoller>();
            }

            ranchMap.Initialize(this, mapWidth, mapHeight, tileSprite, animalSprite, sceneTiles);
            SeedAnimals(startingAnimals);
            SelectCell(null);
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
            if (waitingToEnterNextDay)
            {
                EnterNextDay();
                return;
            }

            if (waitingForOfferSelection)
            {
                return;
            }

            ResolveDailySettlement();
            if (testMode)
            {
                waitingToEnterNextDay = true;
                NotifyStateChanged();
                return;
            }

            RollOffers(3);
            if (currentOffers.Count > 0)
            {
                waitingForOfferSelection = true;
            }
            else
            {
                waitingToEnterNextDay = true;
            }

            NotifyStateChanged();
        }

        public void AddMoney(int amount)
        {
            AddExtraMoney(activeExtraMoneyOwner, amount);
        }

        public void AddExtraMoney(Animal source, int amount)
        {
            if (source != null)
            {
                amount = source.ResolveExtraMoney(amount);
            }

            money += amount;
        }

        public bool TryMoveAnimal(Animal animal, Vector2Int targetCoords)
        {
            if (animal == null)
            {
                return false;
            }

            var startCoords = animal.Coords;
            if (!ranchMap.TryMoveAnimal(animal, targetCoords))
            {
                return false;
            }

            if (animal.Coords != startCoords)
            {
                ResolveMovedAbility(animal);
            }

            return true;
        }

        public bool TrySwapAnimals(Animal first, Animal second)
        {
            if (first == null || second == null)
            {
                return false;
            }

            var firstStartCoords = first.Coords;
            var secondStartCoords = second.Coords;
            if (!ranchMap.TrySwapAnimals(first, second))
            {
                return false;
            }

            if (first.Coords != firstStartCoords)
            {
                ResolveMovedAbility(first);
            }

            if (second.Coords != secondStartCoords)
            {
                ResolveMovedAbility(second);
            }

            return true;
        }

        public bool RemoveAnimal(Animal animal)
        {
            if (animal == null)
            {
                return false;
            }

            var removedFromMap = ranchMap.TryRemoveAnimal(animal);
            var removedFromList = animals.Remove(animal);
            if (selectedCell != null && selectedCell.Animal == null)
            {
                SelectCell(null);
            }

            return removedFromMap || removedFromList;
        }

        public bool ReplaceAnimal(Animal oldAnimal, AnimalData newAnimalData)
        {
            if (oldAnimal == null || newAnimalData == null)
            {
                return false;
            }

            var coords = oldAnimal.Coords;
            if (!RemoveAnimal(oldAnimal))
            {
                return false;
            }

            var newAnimal = new Animal(newAnimalData, coords);
            if (!ranchMap.TryPlaceAnimal(newAnimal, coords))
            {
                return false;
            }

            animals.Add(newAnimal);
            return true;
        }

        public bool TrySetAnimalAt(Vector2Int coords, AnimalData animalData)
        {
            if (ranchMap == null || animalData == null || !ranchMap.TryGetCell(coords, out var cell))
            {
                return false;
            }

            if (cell.Animal != null)
            {
                RemoveAnimal(cell.Animal);
            }

            var animal = new Animal(animalData, coords);
            if (!ranchMap.TryPlaceAnimal(animal, coords))
            {
                return false;
            }

            animals.Add(animal);
            SelectCell(cell);
            NotifyStateChanged();
            return true;
        }

        public bool TryClearAnimalAt(Vector2Int coords)
        {
            if (ranchMap == null || !ranchMap.TryGetCell(coords, out var cell) || cell.Animal == null)
            {
                return false;
            }

            var removed = RemoveAnimal(cell.Animal);
            SelectCell(cell);
            NotifyStateChanged();
            return removed;
        }

        public void ClearAllAnimals()
        {
            if (ranchMap != null)
            {
                foreach (var cell in ranchMap.GetCells())
                {
                    cell.RemoveAnimal();
                }
            }

            animals.Clear();
            SelectCell(null);
            NotifyStateChanged();
        }

        public void EnterTestMode()
        {
            testMode = true;
            waitingForOfferSelection = false;
            waitingToEnterNextDay = false;
            currentOffers.Clear();
            lastSettlementReport = "\u6d4b\u8bd5\u6a21\u5f0f\uff1a\u8bf7\u70b9\u51fb\u5730\u5757\u5e76\u6dfb\u52a0\u52a8\u7269";
            ClearAllAnimals();
        }

        public void ExitTestMode()
        {
            testMode = false;
            waitingForOfferSelection = false;
            waitingToEnterNextDay = false;
            currentOffers.Clear();
            NotifyStateChanged();
        }

        public bool SelectOffer(int index)
        {
            if (!waitingForOfferSelection || index < 0 || index >= currentOffers.Count)
            {
                return false;
            }

            var selectedAnimal = currentOffers[index];
            var added = TryAddAnimalToRandomEmptyCell(selectedAnimal);

            waitingForOfferSelection = false;
            waitingToEnterNextDay = true;
            currentOffers.Clear();
            NotifyStateChanged();
            return added;
        }

        public int CountAnimalsById(string animalId)
        {
            if (string.IsNullOrWhiteSpace(animalId))
            {
                return 0;
            }

            return animals.Count(animal => animal.Data != null && string.Equals(animal.Data.Id, animalId, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasAnimalById(string animalId)
        {
            return CountAnimalsById(animalId) > 0;
        }

        public string GetSelectedCellText()
        {
            if (selectedCell == null)
            {
                return "Select a cell";
            }

            if (selectedCell.Animal == null)
            {
                return $"Cell {selectedCell.Coords.x},{selectedCell.Coords.y}: empty";
            }

            var animal = selectedCell.Animal;
            var ability = animal.Ability != null ? $" ability {animal.Ability.Name}" : string.Empty;
            return $"{animal.DisplayName}: {animal.BaseMoney:+#;-#;0} gold/day, age {animal.AgeDays}{ability}";
        }

        private void SeedAnimals(IReadOnlyList<AnimalData> startingAnimals)
        {
            animals.Clear();
            if (startingAnimals == null || startingAnimals.Count == 0)
            {
                return;
            }

            if (offerPool.Count == 0)
            {
                offerPool = startingAnimals.Where(data => data != null).Distinct().ToList();
            }

            for (var i = 0; i < startingAnimals.Count; i++)
            {
                TryAddAnimalToRandomEmptyCell(startingAnimals[i]);
            }
        }

        private void EnterNextDay()
        {
            RandomizeAnimalPositions();
            day++;
            waitingToEnterNextDay = false;
            NotifyStateChanged();
        }

        private void NotifyStateChanged()
        {
            StateChanged?.Invoke();
        }

        private void ResolveDailySettlement()
        {
            BeginSettlementReport();

            ResolveAbilitiesByMapScan("SettlementPrepare");
            ResolveAbilitiesByMapScan("DayStart");
            ResolveAbilitiesByMapScan("DayEnd");

            var income = 0;
            foreach (var cell in ranchMap.GetCellsInScanOrder())
            {
                var animal = cell.Animal;
                if (animal == null)
                {
                    continue;
                }

                var resolvedMoney = animal.ResolveBaseMoney(ranchMap);
                income += resolvedMoney;
                var report = GetSettlementAnimalReport(animal);
                report.BaseMoney += resolvedMoney;
            }

            money += income;
            BuildCompactSettlementReport();
            lastSettlementReport = settlementReportBuilder.ToString();
        }

        private void ResolveAbilitiesByMapScan(string triggerType)
        {
            var executedAnimals = new HashSet<Animal>();
            foreach (var cell in ranchMap.GetCellsInScanOrder())
            {
                var animal = cell.Animal;
                if (animal == null || executedAnimals.Contains(animal))
                {
                    continue;
                }

                executedAnimals.Add(animal);
                GetSettlementAnimalReport(animal);
                ExecuteAbilityWithReport(animal, triggerType);
            }
        }

        private void ResolveMovedAbility(Animal animal)
        {
            ExecuteAbilityWithReport(animal, "Moved");
        }

        private void ExecuteAbilityWithReport(Animal animal, string triggerType)
        {
            if (animal == null || animal.Ability == null)
            {
                return;
            }

            var beforeMoney = money;
            var previousExtraMoneyOwner = activeExtraMoneyOwner;
            activeExtraMoneyOwner = animal;
            try
            {
                animal.Ability.TryExecute(new AnimalAbilityContext(this, animal), triggerType);
            }
            finally
            {
                activeExtraMoneyOwner = previousExtraMoneyOwner;
            }

            var moneyDelta = money - beforeMoney;
            if (moneyDelta == 0)
            {
                return;
            }

            if (moneyDelta != 0)
            {
                var report = GetSettlementAnimalReport(animal);
                report.AbilityMoney += moneyDelta;
            }
        }

        private void BeginSettlementReport()
        {
            settlementReportBuilder.Clear();
            settlementAnimalReports.Clear();
            settlementReportByAnimal.Clear();
            foreach (var animal in animals)
            {
                animal.ResetSettlementModifiers();
            }
        }

        private void AppendSettlementLine(string line)
        {
            if (settlementReportBuilder.Length > 0)
            {
                settlementReportBuilder.AppendLine();
            }

            settlementReportBuilder.Append(line);
        }

        private SettlementAnimalReport GetSettlementAnimalReport(Animal animal)
        {
            if (settlementReportByAnimal.TryGetValue(animal, out var report))
            {
                return report;
            }

            report = new SettlementAnimalReport
            {
                Name = animal != null ? animal.DisplayName : "\u672a\u77e5"
            };
            settlementReportByAnimal.Add(animal, report);
            settlementAnimalReports.Add(report);
            return report;
        }

        private void BuildCompactSettlementReport()
        {
            settlementReportBuilder.Clear();
            var total = 0;
            for (var i = 0; i < settlementAnimalReports.Count; i++)
            {
                var report = settlementAnimalReports[i];
                total += report.AbilityMoney + report.BaseMoney;
                AppendSettlementLine($"{i + 1}\u3001{report.Name}\uff1a\u80fd\u529b{report.AbilityMoney}\uff0c\u57fa\u7840{report.BaseMoney}");
            }

            AppendSettlementLine($"\u5171\uff1a{total}");
        }

        private class SettlementAnimalReport
        {
            public string Name;
            public int AbilityMoney;
            public int BaseMoney;
        }

        private void RollOffers(int count)
        {
            currentOffers.Clear();
            if (offerRoller != null)
            {
                currentOffers.AddRange(offerRoller.Roll(day, count, offerPool));
                return;
            }

            RollOffersWithoutRoller(count);
        }

        private void RollOffersWithoutRoller(int count)
        {
            if (offerPool == null || offerPool.Count == 0)
            {
                return;
            }

            var validPool = offerPool.Where(data => data != null).ToList();
            if (validPool.Count == 0)
            {
                return;
            }

            var shuffledPool = validPool.OrderBy(_ => UnityEngine.Random.value).ToList();
            for (var i = 0; i < count && i < shuffledPool.Count; i++)
            {
                currentOffers.Add(shuffledPool[i]);
            }
        }

        private bool TryAddAnimalToRandomEmptyCell(AnimalData data)
        {
            if (data == null)
            {
                return false;
            }

            var emptyCells = ranchMap.GetCells()
                .Where(cell => cell != null && cell.IsEmpty)
                .ToList();
            if (emptyCells.Count == 0)
            {
                return false;
            }

            var cell = emptyCells[UnityEngine.Random.Range(0, emptyCells.Count)];
            var animal = new Animal(data, cell.Coords);
            if (!cell.TryPlaceAnimal(animal))
            {
                return false;
            }

            animals.Add(animal);
            return true;
        }

        private void RandomizeAnimalPositions()
        {
            if (animals.Count <= 1)
            {
                return;
            }

            var allCells = ranchMap.GetCells().Where(cell => cell != null).ToList();
            if (allCells.Count == 0)
            {
                return;
            }

            foreach (var cell in allCells)
            {
                if (!cell.IsEmpty)
                {
                    cell.RemoveAnimal();
                }
            }

            var shuffledCells = allCells.OrderBy(_ => UnityEngine.Random.value).ToList();
            var maxPlaceCount = Mathf.Min(animals.Count, shuffledCells.Count);
            for (var i = 0; i < maxPlaceCount; i++)
            {
                shuffledCells[i].TryPlaceAnimal(animals[i]);
            }
        }
    }
}
