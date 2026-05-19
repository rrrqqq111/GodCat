using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Ranch;
using UnityEngine;

namespace NekogamiRanch.Toys
{
    public class RanchToyService
    {
        private readonly RanchManager manager;
        private readonly RanchEconomyService economy;
        private readonly List<ToyData> equippedToys = new List<ToyData>();

        public RanchToyService(RanchManager manager, RanchEconomyService economy, IReadOnlyList<ToyData> toys)
        {
            this.manager = manager;
            this.economy = economy;
            Register(toys);
        }

        public IReadOnlyList<ToyData> EquippedToys => equippedToys;

        public IReadOnlyList<string> EquippedToyIds => equippedToys
            .Where(toy => toy != null)
            .Select(toy => toy.Id)
            .ToList();

        public void Register(IReadOnlyList<ToyData> toys)
        {
            equippedToys.Clear();
            if (toys == null)
            {
                return;
            }

            foreach (var toy in toys)
            {
                Register(toy);
            }
        }

        public bool Register(ToyData toy)
        {
            if (toy == null || !toy.EnabledInDemo)
            {
                return false;
            }

            if (toy.Unique && equippedToys.Any(registered => registered != null && registered.Id == toy.Id))
            {
                return false;
            }

            equippedToys.Add(toy);
            return true;
        }

        public bool IsEquipped(string toyId)
        {
            return TryGetToyById(toyId, out _);
        }

        public bool TryGetToyById(string toyId, out ToyData toy)
        {
            toy = null;
            if (string.IsNullOrWhiteSpace(toyId))
            {
                return false;
            }

            toy = equippedToys.FirstOrDefault(candidate => candidate != null && candidate.Id == toyId);
            return toy != null;
        }

        public void Trigger(ToyTriggerType triggerType, int day)
        {
            var context = new ToyUseContext(manager, economy, day, triggerType);
            foreach (var toy in equippedToys
                .Where(toy => toy != null && toy.TriggerType == triggerType)
                .OrderBy(toy => toy.Priority))
            {
                if (string.IsNullOrWhiteSpace(toy.EffectScriptId))
                {
                    continue;
                }

                if (!ToyEffectRegistry.TryCreate(toy.EffectScriptId, out var effect))
                {
                    Debug.LogWarning($"[RanchToyService] Missing toy effect: {toy.EffectScriptId}");
                    continue;
                }

                var result = effect.TryExecute(toy, context);
                if (result.Success && !string.IsNullOrWhiteSpace(result.Message))
                {
                    Debug.Log($"[RanchToyService] {result.Message}");
                }
            }
        }
    }
}
