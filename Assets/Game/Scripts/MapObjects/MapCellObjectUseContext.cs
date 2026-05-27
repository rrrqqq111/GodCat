using NekogamiRanch.Animals;
using NekogamiRanch.Ranch;

namespace NekogamiRanch.MapObjects
{
    public readonly struct MapCellObjectUseContext
    {
        public MapCellObjectUseContext(RanchManager ranchManager, Animal consumer, MapCellObjectRuntime mapObject)
        {
            RanchManager = ranchManager;
            Consumer = consumer;
            MapObject = mapObject;
        }

        public RanchManager RanchManager { get; }
        public Animal Consumer { get; }
        public MapCellObjectRuntime MapObject { get; }
    }
}
