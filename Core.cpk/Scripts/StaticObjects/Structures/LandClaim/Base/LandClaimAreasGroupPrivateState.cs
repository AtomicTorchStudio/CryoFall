namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class LandClaimAreasGroupPrivateState : BasePrivateState
    {
        /// <summary>
        /// Is the base was founded by a demo player?
        /// In that case a shortened decay duration should apply.
        /// </summary>
        [SyncToClient]
        [TempOnly]
        public bool IsFounderDemoPlayer { get; set; }

        [SyncToClient]
        public IItemsContainer ItemsContainer { get; set; }

        [SyncToClient]
        public ILogicObject PowerGrid { get; set; }

        public List<ILogicObject> ServerLandClaimsAreas { get; set; }
    }
}