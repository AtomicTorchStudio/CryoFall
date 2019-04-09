namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ProtoBarrelPrivateState : ObjectManufacturerPrivateState
    {
        [SyncToClient(DeliveryMode.UnreliableSequenced)]
        public ushort LiquidAmount { get; set; }

        [SyncToClient]
        public LiquidType? LiquidType { get; set; }
    }
}