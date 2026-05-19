using System;
using UnityEngine;

namespace NekogamiRanch.Abilities
{
    public class ConfiguredAnimalAbility : IAnimalAbility
    {
        private readonly AbilityData config;
        private int triggerCount;

        public ConfiguredAnimalAbility(AbilityData abilityData)
        {
            config = abilityData;
        }

        public string Name => config != null ? config.Id : string.Empty;
        public int Priority => config != null ? config.Priority : 0;

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

            var delayDays = config.EffectParams != null ? config.EffectParams.delayDays : 0;
            if (delayDays > 0 && context.Owner.AgeDays < delayDays)
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
    }
}
