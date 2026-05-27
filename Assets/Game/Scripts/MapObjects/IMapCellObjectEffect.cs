namespace NekogamiRanch.MapObjects
{
    public interface IMapCellObjectEffect
    {
        MapCellObjectUseResult TryExecute(MapCellObjectRuntime mapObject, MapCellObjectUseContext context);
    }
}
