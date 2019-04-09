namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights
{
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectLightPrivateState : StructurePrivateState
    {
        [SyncToClient]
        public IItemsContainer ContainerInput { get; set; }

        [SyncToClient(
            deliveryMode: DeliveryMode.ReliableSequenced,
            maxUpdatesPerSecond: 1,
            networkDataType: typeof(float))]
        public double FuelAmount { get; set; }

        [SyncToClient]
        public ObjectLightMode Mode { get; set; }
    }
}