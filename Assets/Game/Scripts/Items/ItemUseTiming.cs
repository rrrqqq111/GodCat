using UnityEngine;

namespace NekogamiRanch.Items
{
    public enum ItemUseTiming
    {
        [InspectorName("被动")]
        Passive = 0,
        [InspectorName("任意时机")]
        Anytime = 1,
        [InspectorName("结算前")]
        BeforeSettlement = 2,
        [InspectorName("结算中")]
        DuringSettlement = 3,
        [InspectorName("结算后")]
        AfterSettlement = 4,
        [InspectorName("动物选择阶段")]
        OfferSelection = 5,
        [InspectorName("进入下一天阶段")]
        DayTransition = 6,
        [InspectorName("商店")]
        Shop = 7,
        [InspectorName("仅测试")]
        TestOnly = 99
    }
}
