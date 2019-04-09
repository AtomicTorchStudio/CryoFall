namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public interface IFoodPrivateState : IPrivateState
    {
        uint FreshnessCurrent { get; set; }
    }
}