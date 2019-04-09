namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectManufacturerPrivateState : StructurePrivateState
    {
        [SyncToClient]
        public CraftingQueue FuelBurningByproductsQueue { get; set; }

        [SyncToClient]
        public FuelBurningState FuelBurningState { get; set; }

        [SyncToClient]
        public ManufacturingState ManufacturingState { get; set; }
    }
}