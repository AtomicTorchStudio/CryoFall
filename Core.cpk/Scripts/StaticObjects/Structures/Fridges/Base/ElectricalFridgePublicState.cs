namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ElectricalFridgePublicState : ObjectCratePublicState, IObjectElectricityConsumerPublicState
    {
        [SyncToClient]
        public ElectricityConsumerState ElectricityConsumerState { get; set; }
    }
}