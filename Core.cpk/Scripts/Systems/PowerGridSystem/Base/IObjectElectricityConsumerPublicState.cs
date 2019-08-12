namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public interface IObjectElectricityConsumerPublicState : IPublicState
    {
        ElectricityConsumerState ElectricityConsumerState { get; set; }
    }
}