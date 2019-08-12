namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators
{
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectGeneratorSolarPublicState : ObjectGeneratorPublicState
    {
        [SyncToClient]
        public IItemsContainer PanelsContainer { get; set; }
    }
}