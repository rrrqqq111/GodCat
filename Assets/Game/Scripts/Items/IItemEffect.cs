namespace NekogamiRanch.Items
{
    public interface IItemEffect
    {
        ItemUseResult TryExecute(ItemRuntimeState item, ItemUseContext context);
    }
}
