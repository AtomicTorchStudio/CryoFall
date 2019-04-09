namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectDisplayCasePublicState
        : StaticObjectPublicState
    {
        [SyncToClient]
        public IItemsContainer ItemsContainer { get; set; }
    }
}