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
        [SerializeField] private int mapWidth = 5;
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
        private string lastSettlementReport = "\u6682\u65e0\u7ed3\u7b97";

        public event Action StateChanged;

        public int Day => day;
        public int Money => money;
        public RanchMap Map => ranchMap;
        public MapCell SelectedCell => selectedCell;
        public bool IsWaitingForOfferSelection => waitingForOfferSelection;
        public bool IsWaitingToEnterNextDay => waitingToEnterNextDay;
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

            BeginSettlementReport();

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
            animal.Ability.TryExecute(new AnimalAbilityContext(this, animal), triggerType);

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
