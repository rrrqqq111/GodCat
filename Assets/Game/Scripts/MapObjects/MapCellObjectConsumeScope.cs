using UnityEngine;

namespace NekogamiRanch.MapObjects
{
    public enum MapCellObjectConsumeScope
    {
        [InspectorName("自身地块")]
        Self = 0,
        [InspectorName("相邻地块")]
        Adjacent = 1,
        [InspectorName("自身和相邻")]
        SelfAndAdjacent = 2,
        [InspectorName("任意地块")]
        Any = 3
    }
}
