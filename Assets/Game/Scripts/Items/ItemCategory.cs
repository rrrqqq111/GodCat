using UnityEngine;

namespace NekogamiRanch.Items
{
    public enum ItemCategory
    {
        [InspectorName("消耗品")]
        Consumable = 0,
        [InspectorName("遗物")]
        Relic = 1,
        [InspectorName("设施")]
        Facility = 2,
        [InspectorName("地块")]
        Tile = 3,
        [InspectorName("补充包")]
        Pack = 4,
        [InspectorName("天赋")]
        Talent = 5,
        [InspectorName("优惠券")]
        Coupon = 6,
        [InspectorName("调试")]
        Debug = 99
    }
}
