namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ProtoBarrelPublicState : ObjectManufacturerPublicState
    {
        [SyncToClient]
        public LiquidType? LiquidType { get; set; }
    }
}