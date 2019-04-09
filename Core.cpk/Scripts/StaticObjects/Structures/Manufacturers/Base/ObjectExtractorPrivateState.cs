namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectExtractorPrivateState : ObjectManufacturerPrivateState
    {
        [SyncToClient]
        public LiquidContainerState LiquidContainerState { get; set; }
    }
}