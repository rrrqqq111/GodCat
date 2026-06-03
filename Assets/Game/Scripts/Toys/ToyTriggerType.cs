using UnityEngine;

namespace NekogamiRanch.Toys
{
    public enum ToyTriggerType
    {
        [InspectorName("无")]
        None = 0,
        [InspectorName("开局开始")]
        RunStart = 1,
        [InspectorName("白天开始")]
        DayStart = 2,
        [InspectorName("阶段开始")]
        StageStart = 3,
        [InspectorName("阶段结束")]
        StageEnd = 4,
        [InspectorName("动物选项刷新")]
        AnimalOfferRolled = 5,
        [InspectorName("选择动物")]
        AnimalSelected = 6,
        [InspectorName("动物移除")]
        AnimalRemoved = 7,
        [InspectorName("动物捕食成功")]
        AnimalPreySucceeded = 8,
        [InspectorName("商店刷新")]
        ShopRefresh = 9,
        [InspectorName("商店道具刷新")]
        ShopItemRolled = 10,
        [InspectorName("地形收益")]
        TerrainIncome = 11,
        [InspectorName("道具收益")]
        ItemIncome = 12,
        [InspectorName("利息结算")]
        InterestSettlement = 13,
        [InspectorName("自定义")]
        Custom = 100,
    }
}
