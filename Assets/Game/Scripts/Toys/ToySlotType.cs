using UnityEngine;

namespace NekogamiRanch.Toys
{
    public enum ToySlotType
    {
        [InspectorName("普通槽位")]
        Normal = 0,
        [InspectorName("首领槽位")]
        Boss = 1,
        [InspectorName("调试槽位")]
        Debug = 99,
    }
}
