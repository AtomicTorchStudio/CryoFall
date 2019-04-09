namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectOilRefineryPrivateState : ObjectManufacturerPrivateState
    {
        [TempOnly]
        public bool IsLiquidStatesChanged { get; set; }

        [SyncToClient]
        public LiquidContainerState LiquidStateGasoline { get; set; }

        [SyncToClient]
        public LiquidContainerState LiquidStateMineralOil { get; set; }

        [SyncToClient]
        public LiquidContainerState LiquidStateRawPetroleum { get; set; }

        [SyncToClient]
        public ManufacturingState ManufacturingStateGasoline { get; set; }

        [SyncToClient]
        public ManufacturingState ManufacturingStateMineralOil { get; set; }
    }
}