using System;
using System.Collections.Generic;
using NekogamiRanch.Ranch;
using NekogamiRanch.MapObjects;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public class ConfiguredAnimalAbility : IAnimalAbility, IAnimalCooldownStatus, IMapCellObjectConsumerAbility
    {
        private readonly AbilityData config;
        private readonly Dictionary<AbilityData, AbilityRuntimeState> runtimeStates = new Dictionary<AbilityData, AbilityRuntimeState>();

        public ConfiguredAnimalAbility(AbilityData abilityData)
        {
            config = abilityData;
        }

        public string Name => config != null ? config.Id : string.Empty;
        public int Priority => config != null ? config.Priority : 0;
        public bool HasCooldown => HasCooldownConfigured(config);
        public bool CanConsumeMapCellObjects => config != null && CanConsumeMapObjectsByEffectType(config.EffectType);
        public int RemainingCooldown
        {
            get
            {
                if (!HasCooldown || config == null)
                {
                    return 0;
                }

                var state = GetState(config);
                var cooldownDays = GetCooldownDays(config);
                return Mathf.Max(0, state.RemainingCooldown < 0
                    ? GetInitialCooldownDays(config, cooldownDays)
                    : state.RemainingCooldown);
            }
        }

        public AbilityExecutionResult TryExecute(AnimalAbilityContext context, string triggerType)
        {
            if (config == null || context.Owner == null || context.RanchManager == null)
            {
                return AbilityExecutionResult.Failed(config != null ? config.Id : string.Empty, triggerType);
            }

            var succeeded = false;
            var totalTargetCount = 0;
            foreach (var abilityConfig in EnumerateConfigs(config))
            {
                if (!string.Equals(abilityConfig.TriggerType, triggerType, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var result = TryExecuteConfiguredAbility(abilityConfig, context, triggerType);
                succeeded |= result.Success;
                totalTargetCount += result.TargetCount;
            }

            if (!succeeded)
            {
                return AbilityExecutionResult.Failed(config.Id, triggerType, totalTargetCount);
            }

            context.RanchManager.NotifyAnimalAbilitySucceeded(context.Owner, config);
            return AbilityExecutionResult.Succeeded(config.Id, triggerType, totalTargetCount);
        }

        private AbilityExecutionResult TryExecuteConfiguredAbility(AbilityData abilityConfig, AnimalAbilityContext context, string triggerType)
        {
            var state = GetState(abilityConfig);
            if (abilityConfig.TriggerLimit > 0 && state.TriggerCount >= abilityConfig.TriggerLimit)
            {
                return AbilityExecutionResult.Failed(abilityConfig.Id, triggerType);
            }

            if (IsCoolingDown(abilityConfig, state, context))
            {
                return AbilityExecutionResult.Failed(abilityConfig.Id, triggerType);
            }

            var chancePercent = Mathf.Clamp(abilityConfig.TriggerChancePercent, 0, 100);
            if (chancePercent < 100 && UnityEngine.Random.Range(0, 100) >= chancePercent)
            {
#if UNITY_EDITOR
                Debug.Log($"[Ability] {abilityConfig.Id} missed by chance ({chancePercent}%). owner={context.Owner.DisplayName}");
#endif
                return AbilityExecutionResult.Failed(abilityConfig.Id, triggerType);
            }

            if (!AbilityEffectRegistry.TryGet(abilityConfig.EffectScriptId, out var effect))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[Ability] effectScriptId '{abilityConfig.EffectScriptId}' not registered. ability={abilityConfig.Id}");
#endif
                return AbilityExecutionResult.Failed(abilityConfig.Id, triggerType);
            }

            var targets = AbilityTargetResolver.Resolve(context, abilityConfig);
            if (effect.Execute(context, abilityConfig, targets))
            {
                state.TriggerCount++;
                ResetCooldownAfterSuccess(abilityConfig, state, context);
#if UNITY_EDITOR
                Debug.Log($"[Ability] {abilityConfig.Id} executed. owner={context.Owner.DisplayName} targets={targets.Count} triggerCount={state.TriggerCount}");
#endif
                return AbilityExecutionResult.Succeeded(abilityConfig.Id, triggerType, targets.Count);
            }
#if UNITY_EDITOR
            else
            {
                Debug.Log($"[Ability] {abilityConfig.Id} attempted but no effect applied. owner={context.Owner.DisplayName} targets={targets.Count}");
            }
#endif
            return AbilityExecutionResult.Failed(abilityConfig.Id, triggerType, targets.Count);
        }

        private bool IsCoolingDown(AbilityData abilityConfig, AbilityRuntimeState state, AnimalAbilityContext context)
        {
            var cooldownDays = GetCooldownDays(abilityConfig);
            var initialCooldownDays = GetInitialCooldownDays(abilityConfig, cooldownDays);
            if (cooldownDays <= 0 && initialCooldownDays <= 0)
            {
                return false;
            }

            if (state.RemainingCooldown < 0)
            {
                state.RemainingCooldown = initialCooldownDays;
            }

            if (state.RemainingCooldown <= 0)
            {
                return false;
            }

            ReduceCooldown(abilityConfig, state, context, 1, "NaturalDailyCooldownReduction");
            ReduceCooldown(abilityConfig, state, context, GetTileCooldownReductionAmount(abilityConfig, context), "TileCooldownBonusReduction");
            return state.RemainingCooldown > 0;
        }

        private void ResetCooldownAfterSuccess(AbilityData abilityConfig, AbilityRuntimeState state, AnimalAbilityContext context)
        {
            var cooldownDays = GetCooldownDays(abilityConfig);
            if (cooldownDays <= 0)
            {
                state.RemainingCooldown = 0;
                return;
            }

            state.RemainingCooldown = cooldownDays;
            ReduceCooldown(abilityConfig, state, context, GetTileCooldownReductionAmount(abilityConfig, context), "TileCooldownBonusReduction");
        }

        private static int GetInitialCooldownDays(AbilityData abilityConfig, int cooldownDays)
        {
            if (abilityConfig?.EffectParams == null)
            {
                return 0;
            }

            return Mathf.Max(0, abilityConfig.EffectParams.initialCooldownDays > 0
                ? abilityConfig.EffectParams.initialCooldownDays
                : cooldownDays);
        }

        private static int GetCooldownDays(AbilityData abilityConfig)
        {
            return abilityConfig?.EffectParams != null ? Mathf.Max(0, abilityConfig.EffectParams.cooldownDays) : 0;
        }

        private static bool HasCooldownConfigured(AbilityData abilityConfig)
        {
            var cooldownDays = GetCooldownDays(abilityConfig);
            return cooldownDays > 0 || GetInitialCooldownDays(abilityConfig, cooldownDays) > 0;
        }

        private static bool CanConsumeMapObjectsByEffectType(string effectType)
        {
            if (string.IsNullOrWhiteSpace(effectType))
            {
                return false;
            }

            return effectType.IndexOf("Prey", StringComparison.OrdinalIgnoreCase) >= 0 ||
                effectType.IndexOf("Ambush", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private int GetTileCooldownReductionAmount(AbilityData abilityConfig, AnimalAbilityContext context)
        {
            var effectParams = abilityConfig.EffectParams;
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

        private void ReduceCooldown(AbilityData abilityConfig, AbilityRuntimeState state, AnimalAbilityContext context, int amount, string reason)
        {
            if (amount <= 0 || state.RemainingCooldown <= 0)
            {
                return;
            }

            var previousCooldown = state.RemainingCooldown;
            state.RemainingCooldown = Mathf.Max(0, state.RemainingCooldown - amount);
            context.RanchManager.NotifyAnimalCooldownReduced(new AnimalCooldownReductionContext(
                context.Owner,
                previousCooldown - state.RemainingCooldown,
                previousCooldown,
                state.RemainingCooldown,
                context.Owner,
                abilityConfig.Id,
                reason));
        }

        private AbilityRuntimeState GetState(AbilityData abilityConfig)
        {
            if (!runtimeStates.TryGetValue(abilityConfig, out var state))
            {
                state = new AbilityRuntimeState();
                runtimeStates.Add(abilityConfig, state);
            }

            return state;
        }

        private static IEnumerable<AbilityData> EnumerateConfigs(AbilityData abilityConfig)
        {
            if (abilityConfig == null)
            {
                yield break;
            }

            yield return abilityConfig;
            foreach (var subAbility in abilityConfig.SubAbilities)
            {
                foreach (var nestedAbility in EnumerateConfigs(subAbility))
                {
                    yield return nestedAbility;
                }
            }
        }

        private class AbilityRuntimeState
        {
            public int TriggerCount;
            public int RemainingCooldown = -1;
        }
    }
}
