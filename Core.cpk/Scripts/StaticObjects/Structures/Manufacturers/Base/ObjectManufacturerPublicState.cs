namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectManufacturerPublicState : StaticObjectPublicState
    {
        [SyncToClient(DeliveryMode.ReliableSequenced)]
        public bool IsManufacturingActive { get; set; }
    }
}