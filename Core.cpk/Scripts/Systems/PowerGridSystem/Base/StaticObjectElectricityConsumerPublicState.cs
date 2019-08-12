namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class StaticObjectElectricityConsumerPublicState
        : StaticObjectPublicState, IObjectElectricityConsumerPublicState
    {
        [SyncToClient]
        public ElectricityConsumerState ElectricityConsumerState { get; set; }
    }
}