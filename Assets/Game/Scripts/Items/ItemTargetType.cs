using UnityEngine;

namespace NekogamiRanch.Items
{
    public enum ItemTargetType
    {
        [InspectorName("无")]
        None = 0,
        [InspectorName("全局")]
        Global = 1,
        [InspectorName("地块")]
        Cell = 2,
        [InspectorName("空地块")]
        EmptyCell = 3,
        [InspectorName("有动物地块")]
        OccupiedCell = 4,
        [InspectorName("动物")]
        Animal = 5,
        [InspectorName("道具")]
        Item = 6,
        [InspectorName("动物选项")]
        ShopOffer = 7
    }
}
