using NekogamiRanch.Animals;
using UnityEngine;

namespace NekogamiRanch.MapObjects
{
    [CreateAssetMenu(menuName = "Nekogami Ranch/Map Cell Object Data", fileName = "MapCellObjectData")]
    public class MapCellObjectData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string objectName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private Sprite icon;

        [Header("Placement")]
        [SerializeField] private MapCellObjectConsumeScope consumeScope = MapCellObjectConsumeScope.Any;
        [SerializeField] private bool consumeOnSuccess = true;

        [Header("Effect")]
        [SerializeField] private string effectScriptId;
        [SerializeField] private MapCellObjectEffectParams effectParams = new MapCellObjectEffectParams();

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
