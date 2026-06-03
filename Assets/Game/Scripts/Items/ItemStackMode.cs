using UnityEngine;

namespace NekogamiRanch.Items
{
    public enum ItemStackMode
    {
        [InspectorName("唯一")]
        Unique = 0,
        [InspectorName("数量堆叠")]
        Count = 1,
        [InspectorName("加法叠加")]
        Additive = 2,
        [InspectorName("乘法叠加")]
        Multiplicative = 3,
        [InspectorName("运行时层数")]
        RuntimeStacks = 4
    }
}
