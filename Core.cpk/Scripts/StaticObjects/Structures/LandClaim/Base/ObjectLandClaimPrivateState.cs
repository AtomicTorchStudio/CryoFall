namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectLandClaimPrivateState : StructurePrivateState
    {
        [SyncToClient]
        public IItemsContainer ItemsContainer { get; set; }
    }
}