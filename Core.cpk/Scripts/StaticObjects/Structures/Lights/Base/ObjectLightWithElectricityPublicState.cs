namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights
{
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectLightWithElectricityPublicState : ObjectLightPublicState, IObjectElectricityConsumerPublicState
    {
        [SyncToClient]
        public ElectricityConsumerState ElectricityConsumerState { get; set; }
    }
}