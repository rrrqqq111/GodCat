using System;
using System.Collections.Generic;
using System.Linq;
using NekogamiRanch.Animals;

namespace NekogamiRanch.Abilities
{
    public class ElkAbilityEffect : IAbilityEffect
    {
        public bool Execute(AnimalAbilityContext context, AbilityData abilityData, IReadOnlyList<Animal> targets)
        {
            if (context.Owner == null || context.RemovedAnimal == null || context.RanchManager?.Map == null)
            {
                return false;
            }

            var recipients = context.RanchManager.Map
                .GetNeighbors(context.Owner.Coords)
                .Select(cell => cell.Animal)
                .Where(animal => animal != null && animal != context.Owner)
                .ToList();
            var sharedMoney = context.RemovedAnimal.BaseMoney / 2;
            if (recipients.Count == 0 || sharedMoney == 0)
            {
                return false;
            }

            var evenlySharedMoney = sharedMoney / recipients.Count;
            var remainder = sharedMoney % recipients.Count;
            foreach (var recipient in recipients)
            {
                var bonus = evenlySharedMoney;
                if (remainder != 0)
                {
                    var remainderUnit = Math.Sign(remainder);
                    bonus += remainderUnit;
                    remainder -= remainderUnit;
                }

                if (bonus != 0)
                {
                    recipient.AddPermanentBaseMoneyBonus(bonus);
                }
            }

            return true;
        }
    }
}
