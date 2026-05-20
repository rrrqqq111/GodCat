using System;
using NekogamiRanch.Ranch;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public class ConfiguredAnimalAbility : IAnimalAbility, IAnimalCooldownStatus
    {
        private readonly AbilityData config;
        private int triggerCount;
        private int remainingCooldown = -1;

        public ConfiguredAnimalAbility(AbilityData abilityData)
        {
            config = abilityData;
        }

        public string Name => config != null ? config.Id : string.Empty;
        public int Priority => config != null ? config.Priority : 0;
        public bool HasCooldown => GetCooldownDays() > 0 || GetInitialCooldownDays(GetCooldownDays()) > 0;
        public int RemainingCooldown => HasCooldown ? Mathf.Max(0, remainingCooldown < 0 ? GetInitialCooldownDays(GetCooldownDays()) : remainingCooldown) : 0;

        public AbilityExecutionResult TryExecute(AnimalAbilityContext context, string triggerType)
        {
            if (config == null || context.Owner == null || context.RanchManager == null)
            {
                return AbilityExecutionResult.Failed(config != null ? config.Id : string.Empty, triggerType);
            }

            if (!string.Equals(config.TriggerType, triggerType, StringComparison.OrdinalIgnoreCase))
            {
                return AbilityExecutionResult.Failed(config.Id, triggerType);
            }

            if (config.TriggerLimit > 0 && triggerCount >= config.TriggerLimit)
            {
                return AbilityExecutionResult.Failed(config.Id, triggerType);
            }

            if (IsCoolingDown(context))
            {
                return AbilityExecutionResult.Failed(config.Id, triggerType);
            }

            var chancePercent = Mathf.Clamp(config.TriggerChancePercent, 0, 100);
            if (chancePercent < 100 && UnityEngine.Random.Range(0, 100) >= chancePercent)
            {
#if UNITY_EDITOR
                Debug.Log($"[Ability] {config.Id} missed by chance ({chancePercent}%). owner={context.Owner.DisplayName}");
#endif
                return AbilityExecutionResult.Failed(config.Id, triggerType);
            }

            if (!AbilityEffectRegistry.TryGet(config.EffectScriptId, out var effect))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[Ability] effectScriptId '{config.EffectScriptId}' not registered. ability={config.Id}");
#endif
                return AbilityExecutionResult.Failed(config.Id, triggerType);
            }

            var targets = AbilityTargetResolver.Resolve(context, config);
            if (effect.Execute(context, config, targets))
            {
                triggerCount++;
                ResetCooldownAfterSuccess(context);
#if UNITY_EDITOR
                Debug.Log($"[Ability] {config.Id} executed. owner={context.Owner.DisplayName} targets={targets.Count} triggerCount={triggerCount}");
#endif
                return AbilityExecutionResult.Succeeded(config.Id, triggerType, targets.Count);
            }
#if UNITY_EDITOR
            else
            {
                Debug.Log($"[Ability] {config.Id} attempted but no effect applied. owner={context.Owner.DisplayName} targets={targets.Count}");
            }
#endif
            return AbilityExecutionResult.Failed(config.Id, triggerType, targets.Count);
        }

        private bool IsCoolingDown(AnimalAbilityContext context)
        {
            var cooldownDays = GetCooldownDays();
            var initialCooldownDays = GetInitialCooldownDays(cooldownDays);
            if (cooldownDays <= 0 && initialCooldownDays <= 0)
            {
                return false;
            }

            if (remainingCooldown < 0)
            {
                remainingCooldown = initialCooldownDays;
            }

            if (remainingCooldown <= 0)
            {
                return false;
            }

            ReduceCooldown(context, 1, "NaturalDailyCooldownReduction");
            ReduceCooldown(context, GetTileCooldownReductionAmount(context), "TileCooldownBonusReduction");
            return remainingCooldown > 0;
        }

        private void ResetCooldownAfterSuccess(AnimalAbilityContext context)
        {
            var cooldownDays = GetCooldownDays();
            if (cooldownDays <= 0)
            {
                remainingCooldown = 0;
                return;
            }

            remainingCooldown = cooldownDays;
            ReduceCooldown(context, GetTileCooldownReductionAmount(context), "TileCooldownBonusReduction");
        }

        private int GetInitialCooldownDays(int cooldownDays)
        {
            if (config.EffectParams == null)
            {
                return 0;
            }

            return Mathf.Max(0, config.EffectParams.initialCooldownDays > 0
                ? config.EffectParams.initialCooldownDays
                : cooldownDays);
        }

        private int GetCooldownDays()
        {
            return config.EffectParams != null ? Mathf.Max(0, config.EffectParams.cooldownDays) : 0;
        }

        private int GetTileCooldownReductionAmount(AnimalAbilityContext context)
        {
            var effectParams = config.EffectParams;
            if (effectParams == null ||
                effectParams.cooldownReductionAmount <= 0 ||
                string.IsNullOrWhiteSpace(effectParams.cooldownReductionTileType) ||
                string.Equals(effectParams.cooldownReductionTileType, "None", StringComparison.OrdinalIgnoreCase) ||
                context.RanchManager.Map == null)
            {
                return 0;
            }

            if (!Enum.TryParse<RanchTileType>(effectParams.cooldownReductionTileType, true, out var tileType))
            {
                return 0;
            }

            foreach (var neighbor in context.RanchManager.Map.GetNeighbors(context.Owner.Coords))
            {
                if (context.RanchManager.Map.IsTileType(neighbor.Coords, tileType))
                {
                    return effectParams.cooldownReductionAmount;
                }
            }

            return 0;
        }

        private void ReduceCooldown(AnimalAbilityContext context, int amount, string reason)
        {
            if (amount <= 0 || remainingCooldown <= 0)
            {
                return;
            }

            var previousCooldown = remainingCooldown;
            remainingCooldown = Mathf.Max(0, remainingCooldown - amount);
            context.RanchManager.NotifyAnimalCooldownReduced(new AnimalCooldownReductionContext(
                context.Owner,
                previousCooldown - remainingCooldown,
                previousCooldown,
                remainingCooldown,
                context.Owner,
                config.Id,
                reason));
        }
    }
}
