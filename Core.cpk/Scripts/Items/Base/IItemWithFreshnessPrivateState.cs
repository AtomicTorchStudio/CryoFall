namespace AtomicTorch.CBND.CoreMod.Items
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public interface IItemWithFreshnessPrivateState : IPrivateState
    {
        uint FreshnessCurrent { get; set; }
    }
}