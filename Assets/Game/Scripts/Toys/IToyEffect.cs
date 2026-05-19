namespace NekogamiRanch.Toys
{
    public interface IToyEffect
    {
        ToyUseResult TryExecute(ToyData toy, ToyUseContext context);
    }
}
