using UnityEngine;
using NekogamiRanch.Animals;

namespace NekogamiRanch.MapObjects
{
    public class MapCellObjectRuntime
    {
        public MapCellObjectRuntime(
            string id,
            string displayName,
            Sprite icon,
            MapCellObjectConsumeScope consumeScope,
            bool consumeOnSuccess,
            string effectScriptId,
            MapCellObjectEffectParams effectParams,
            Vector2Int coords,
            Animal sourceAnimal = null,
            int? sourceBaseMoney = null)
        {
            Id = id;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "MapObject" : displayName;
            Icon = icon;
            ConsumeScope = consumeScope;
            ConsumeOnSuccess = consumeOnSuccess;
            EffectScriptId = effectScriptId;
            EffectParams = effectParams ?? new MapCellObjectEffectParams();
            Coords = coords;
            SourceAnimalData = sourceAnimal?.Data;
            SourceBaseMoney = sourceBaseMoney ?? sourceAnimal?.BaseMoney ?? SourceAnimalData?.BaseMoney ?? 0;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public Sprite Icon { get; }
        public MapCellObjectConsumeScope ConsumeScope { get; }
        public bool ConsumeOnSuccess { get; }
        public string EffectScriptId { get; }
        public MapCellObjectEffectParams EffectParams { get; }
        public Vector2Int Coords { get; private set; }
        public AnimalData SourceAnimalData { get; }
        public int SourceBaseMoney { get; }

        public void SetCoords(Vector2Int coords)
        {
            Coords = coords;
        }
    }
}
