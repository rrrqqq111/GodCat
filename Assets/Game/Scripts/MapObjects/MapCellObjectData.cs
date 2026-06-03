using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.MapObjects
{
    [CreateAssetMenu(menuName = "Nekogami Ranch/Map Cell Object Data", fileName = "MapCellObjectData")]
    public class MapCellObjectData : ScriptableObject
    {
        [Header("基础信息")]
        [SerializeField, InspectorName("物体ID")] private string id;
        [SerializeField, InspectorName("物体名称")] private string objectName;
        [SerializeField, TextArea, InspectorName("物体描述")] private string description;
        [SerializeField, InspectorName("物体图标")] private Sprite icon;

        [Header("放置与消耗")]
        [SerializeField, InspectorName("消耗范围")] private MapCellObjectConsumeScope consumeScope = MapCellObjectConsumeScope.Any;
        [SerializeField, InspectorName("成功后消耗")] private bool consumeOnSuccess = true;

        [Header("效果")]
        [SerializeField, InspectorName("效果脚本ID")] private string effectScriptId;
        [SerializeField, InspectorName("效果参数")] private MapCellObjectEffectParams effectParams = new MapCellObjectEffectParams();

        public string Id => string.IsNullOrWhiteSpace(id) ? name : id;
        public string Name => string.IsNullOrWhiteSpace(objectName) ? name : objectName;
        public string DisplayName => Name;
        public string Description => description;
        public Sprite Icon => icon;
        public MapCellObjectConsumeScope ConsumeScope => consumeScope;
        public bool ConsumeOnSuccess => consumeOnSuccess;
        public string EffectScriptId => string.IsNullOrWhiteSpace(effectScriptId) ? id : effectScriptId;
        public MapCellObjectEffectParams EffectParams => effectParams;

        public MapCellObjectRuntime CreateRuntime(Vector2Int coords, Animal sourceAnimal = null)
        {
            return new MapCellObjectRuntime(
                Id,
                DisplayName,
                icon,
                consumeScope,
                consumeOnSuccess,
                EffectScriptId,
                effectParams,
                coords,
                sourceAnimal);
        }
    }
}
