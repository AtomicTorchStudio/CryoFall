namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectMulchboxPrivateState : ObjectManufacturerPrivateState
    {
        [SyncToClient(DeliveryMode.UnreliableSequenced)]
        public ushort OrganicAmount { get; set; }
    }
}