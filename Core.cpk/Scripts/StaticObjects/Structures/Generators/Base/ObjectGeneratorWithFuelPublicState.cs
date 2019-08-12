namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectGeneratorWithFuelPublicState
        : ObjectManufacturerPublicState, IObjectElectricityProducerPublicState
    {
        [SyncToClient(DeliveryMode.ReliableSequenced)]
        public ElectricityProducerState ElectricityProducerState { get; set; }
    }
}