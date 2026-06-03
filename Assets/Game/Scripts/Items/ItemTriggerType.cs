using UnityEngine;

namespace NekogamiRanch.Items
{
    public enum ItemTriggerType
    {
        [InspectorName("手动使用")]
        ManualUse = 0,
        [InspectorName("白天开始")]
        DayStart = 1,
        [InspectorName("结算开始")]
        SettlementStart = 2,
        [InspectorName("结算结束")]
        SettlementEnd = 3,
        [InspectorName("动物添加")]
        AnimalAdded = 4,
        [InspectorName("动物移除")]
        AnimalRemoved = 5,
        [InspectorName("动物出售")]
        AnimalSold = 6,
        [InspectorName("动物移动")]
        AnimalMoved = 7,
        [InspectorName("动物成长")]
        AnimalGrown = 8,
        [InspectorName("动物变形")]
        AnimalTransformed = 9,
        [InspectorName("捕食成功")]
        PreySucceeded = 10,
        [InspectorName("捕食失败")]
        PreyFailed = 11,
        [InspectorName("繁殖成功")]
        BreedSucceeded = 12,
        [InspectorName("地块改变")]
        TileChanged = 13,
        [InspectorName("动物选项刷新")]
        OfferRolled = 14,
        [InspectorName("商店刷新")]
        ShopRefreshed = 15,
        [InspectorName("道具出售")]
        ItemSold = 16,
        [InspectorName("自定义")]
        Custom = 100
    }
}
