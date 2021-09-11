namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    public interface IReadOnlyDropItemsListPreset
    {
        IReadOnlyDropItemsList DropItemsList { get; }

        DropItemRollFunctionDelegate ProbabilityRollFunction { get; }
    }
}