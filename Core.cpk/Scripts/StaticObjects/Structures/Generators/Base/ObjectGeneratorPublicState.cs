namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectGeneratorPublicState : StaticObjectPublicState, IObjectElectricityProducerPublicState
    {
        [SyncToClient(DeliveryMode.ReliableSequenced)]
        public ElectricityProducerState ElectricityProducerState { get; set; }

        /// <summary>
        /// Determines the current generation state (true if generating energy now).
        /// </summary>
        [SyncToClient(DeliveryMode.ReliableSequenced)]
        public bool IsActive { get; set; }
    }
}