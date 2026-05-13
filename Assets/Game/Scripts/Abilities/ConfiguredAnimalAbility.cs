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

        public void TryExecute(AnimalAbilityContext context, string triggerType)
        {
            if (config == null || context.Owner == null || context.RanchManager == null)
            {
                return;
            }

            if (!string.Equals(config.TriggerType, triggerType, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (config.TriggerLimit > 0 && triggerCount >= config.TriggerLimit)
            {
                return;
            }

            var delayDays = config.EffectParams != null ? config.EffectParams.delayDays : 0;
            if (delayDays > 0 && context.Owner.AgeDays < delayDays)
            {
                return;
            }

            var chancePercent = Mathf.Clamp(config.TriggerChancePercent, 0, 100);
            if (chancePercent < 100 && UnityEngine.Random.Range(0, 100) >= chancePercent)
            {
#if UNITY_EDITOR
                Debug.Log($"[Ability] {config.Id} missed by chance ({chancePercent}%). owner={context.Owner.DisplayName}");
#endif
                return;
            }

            if (!AbilityEffectRegistry.TryGet(config.EffectScriptId, out var effect))
            {
#if UNITY_EDITOR
                Debug.LogWarning($"[Ability] effectScriptId '{config.EffectScriptId}' not registered. ability={config.Id}");
#endif
                return;
            }

            var targets = AbilityTargetResolver.Resolve(context, config);
            if (effect.Execute(context, config, targets))
            {
                triggerCount++;
#if UNITY_EDITOR
                Debug.Log($"[Ability] {config.Id} executed. owner={context.Owner.DisplayName} targets={targets.Count} triggerCount={triggerCount}");
#endif
            }
#if UNITY_EDITOR
            else
            {
                Debug.Log($"[Ability] {config.Id} attempted but no effect applied. owner={context.Owner.DisplayName} targets={targets.Count}");
            }
#endif
        }
    }
}
