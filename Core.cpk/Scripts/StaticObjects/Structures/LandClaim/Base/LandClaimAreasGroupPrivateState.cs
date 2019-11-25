namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class LandClaimAreasGroupPrivateState : BasePrivateState
    {
        [SyncToClient]
        public IItemsContainer ItemsContainer { get; set; }

        [SyncToClient]
        public ILogicObject PowerGrid { get; set; }

        public List<ILogicObject> ServerLandClaimsAreas { get; set; }
    }
}