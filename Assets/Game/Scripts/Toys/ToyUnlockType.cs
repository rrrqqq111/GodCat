using UnityEngine;

namespace NekogamiRanch.Toys
{
    public enum ToyUnlockType
    {
        [InspectorName("默认解锁")]
        Always = 0,
        [InspectorName("游戏经验")]
        GameExp = 1,
        [InspectorName("成就")]
        Achievement = 2,
        [InspectorName("标记")]
        Flag = 3,
    }
}
