namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectGeneratorWithFuelPrivateState : ObjectManufacturerPrivateState
    {
        public bool IsLiquidStatesChanged { get; set; }

        [SyncToClient]
        public LiquidContainerState LiquidState { get; set; }
    }
}