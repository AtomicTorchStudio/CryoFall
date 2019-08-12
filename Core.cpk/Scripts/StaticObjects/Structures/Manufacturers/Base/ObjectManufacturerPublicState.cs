namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectManufacturerPublicState
        : StaticObjectPublicState, IObjectElectricityConsumerPublicState, IObjectPublicStateWithActiveFlag
    {
        [SyncToClient]
        public ElectricityConsumerState ElectricityConsumerState { get; set; }

        [SyncToClient(DeliveryMode.ReliableSequenced)]
        public bool IsActive { get; set; }
    }
}