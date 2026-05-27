using NekogamiRanch.Animals;

namespace NekogamiRanch.MapObjects
{
    public class ConsumeMapCellObjectAbilityEffect : IMapCellObjectEffect
    {
        public MapCellObjectUseResult TryExecute(MapCellObjectRuntime mapObject, MapCellObjectUseContext context)
        {
            if (mapObject == null || context.RanchManager == null)
            {
                return MapCellObjectUseResult.Failed();
            }

            if (mapObject.EffectScriptId == "PoopReward")
            {
                if (context.Consumer == null)
                {
                    return MapCellObjectUseResult.Failed("NoConsumer");
                }

                var effectParams = mapObject.EffectParams;
                var moneyMultiplier = effectParams != null && effectParams.moneyMultiplier > 0 ? effectParams.moneyMultiplier : 1;
                var moneyBonus = effectParams != null ? effectParams.moneyBonus : 0;
                var reward = context.Consumer.BaseMoney * moneyMultiplier + moneyBonus;
                if (reward <= 0)
                {
                    return MapCellObjectUseResult.Failed("NoReward");
                }

                context.RanchManager.AddMoney(reward);
                return MapCellObjectUseResult.Succeeded("PoopConsumed", moneyDelta: reward);
            }

            if (mapObject.EffectScriptId == "LeftoverMeatReward")
            {
                if (context.Consumer == null)
                {
                    return MapCellObjectUseResult.Failed("NoConsumer");
                }

                var effectParams = mapObject.EffectParams;
                var sourceBaseMoney = mapObject.SourceBaseMoney;
                var sourcePercent = ResolveSourcePercent(sourceBaseMoney, effectParams);
                var baseMoneyBonus = sourcePercent + ResolveFlatBonus(effectParams);
                if (baseMoneyBonus <= 0)
                {
                    return MapCellObjectUseResult.Failed("NoReward");
                }

                context.Consumer.AddPermanentBaseMoneyBonus(baseMoneyBonus);
                return MapCellObjectUseResult.Succeeded("LeftoverMeatConsumed", baseMoneyBonusDelta: baseMoneyBonus);
            }

            return MapCellObjectUseResult.Failed("MissingObjectEffect");
        }

        private static int ResolveSourcePercent(int sourceBaseMoney, MapCellObjectEffectParams effectParams)
        {
            if (sourceBaseMoney <= 0)
            {
                return 0;
            }

            var minPercent = effectParams != null ? effectParams.sourceBaseMoneyMinPercent : 20;
            var maxPercent = effectParams != null ? effectParams.sourceBaseMoneyMaxPercent : 50;
            if (minPercent > maxPercent)
            {
                var temp = minPercent;
                minPercent = maxPercent;
                maxPercent = temp;
            }

            minPercent = UnityEngine.Mathf.Clamp(minPercent, 0, 100);
            maxPercent = UnityEngine.Mathf.Clamp(maxPercent, 0, 100);
            var percent = UnityEngine.Random.Range(minPercent, maxPercent + 1);
            return sourceBaseMoney * percent / 100;
        }

        private static int ResolveFlatBonus(MapCellObjectEffectParams effectParams)
        {
            return effectParams != null ? effectParams.flatBaseMoneyBonus : 5;
        }
    }
}
