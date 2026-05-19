using System;
using System.Collections.Generic;
using System.Text;
using NekogamiRanch.Abilities;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Ranch
{
    public class RanchSettlementService
    {
        private const int MaxExternalAbilityTriggerDepth = 8;

        private readonly RanchManager manager;
        private readonly RanchAnimalService animalService;
        private readonly RanchEconomyService economyService;
        private readonly StringBuilder settlementReportBuilder = new StringBuilder();
        private readonly List<SettlementAnimalReport> settlementAnimalReports = new List<SettlementAnimalReport>();
        private readonly Dictionary<Animal, SettlementAnimalReport> settlementReportByAnimal = new Dictionary<Animal, SettlementAnimalReport>();
        private Animal activeExtraMoneyOwner;
        private int externalAbilityTriggerDepth;

        public RanchSettlementService(RanchManager manager, RanchAnimalService animalService, RanchEconomyService economyService)
        {
            this.manager = manager;
            this.animalService = animalService;
            this.economyService = economyService;
        }

        public string LastReport { get; private set; } = "暂无结算";

        public void SetLastReport(string report)
        {
            LastReport = report;
        }

        public void AddMoney(int amount)
        {
            if (activeExtraMoneyOwner != null)
            {
                amount = activeExtraMoneyOwner.ResolveExtraMoney(amount);
            }

            economyService.AddMoney(amount);
        }

        public void ResolveDailySettlement(RanchMap ranchMap)
        {
            BeginSettlementReport();

            ResolveAbilitiesByMapScan(ranchMap, "SettlementPrepare");
            ResolveAbilitiesByMapScan(ranchMap, "DayStart");
            ResolveAbilitiesByMapScan(ranchMap, "DayEnd");

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

            economyService.AddMoney(income);
            BuildCompactSettlementReport();
            LastReport = settlementReportBuilder.ToString();
        }

        public AbilityExecutionResult ResolveMovedAbility(Animal animal)
        {
            return ExecuteAbilityWithReport(animal, "Moved");
        }

        public AbilityExecutionResult TryTriggerAnimalAbility(Animal animal)
        {
            var abilityData = animal?.Data?.Ability;
            if (animal == null || animal.Ability == null || abilityData == null || string.IsNullOrWhiteSpace(abilityData.TriggerType))
            {
                return AbilityExecutionResult.Failed(triggerType: abilityData != null ? abilityData.TriggerType : string.Empty);
            }

            if (externalAbilityTriggerDepth >= MaxExternalAbilityTriggerDepth)
            {
                return AbilityExecutionResult.Failed(abilityData.Id, abilityData.TriggerType);
            }

            externalAbilityTriggerDepth++;
            try
            {
                GetSettlementAnimalReport(animal);
                return ExecuteAbilityWithReport(animal, abilityData.TriggerType);
            }
            finally
            {
                externalAbilityTriggerDepth--;
            }
        }

        private void ResolveAbilitiesByMapScan(RanchMap ranchMap, string triggerType)
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

        private AbilityExecutionResult ExecuteAbilityWithReport(Animal animal, string triggerType)
        {
            if (animal == null || animal.Ability == null)
            {
                return AbilityExecutionResult.Failed(triggerType: triggerType);
            }

            var beforeMoney = economyService.Money;
            var previousExtraMoneyOwner = activeExtraMoneyOwner;
            activeExtraMoneyOwner = animal;
            var result = AbilityExecutionResult.Failed(animal.Ability.Name, triggerType);
            try
            {
                result = animal.Ability.TryExecute(new AnimalAbilityContext(manager, animal), triggerType);
            }
            finally
            {
                activeExtraMoneyOwner = previousExtraMoneyOwner;
            }

            var moneyDelta = economyService.Money - beforeMoney;
            if (moneyDelta != 0)
            {
                var report = GetSettlementAnimalReport(animal);
                report.AbilityMoney += moneyDelta;
            }

            return result.WithMoneyDelta(moneyDelta);
        }

        private void BeginSettlementReport()
        {
            settlementReportBuilder.Clear();
            settlementAnimalReports.Clear();
            settlementReportByAnimal.Clear();
            foreach (var animal in animalService.Animals)
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
                Name = animal != null ? animal.DisplayName : "未知"
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
                AppendSettlementLine($"{i + 1}、{report.Name}：能力{report.AbilityMoney}，基础{report.BaseMoney}");
            }

            AppendSettlementLine($"共：{total}");
        }

        private class SettlementAnimalReport
        {
            public string Name;
            public int AbilityMoney;
            public int BaseMoney;
        }
    }
}
