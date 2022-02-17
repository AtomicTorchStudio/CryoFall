namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class LandClaimAreaPublicState : BasePublicState
    {
        [SyncToClient]
        public ILogicObject LandClaimAreasGroup { get; set; }

        [SyncToClient]
        public Vector2Ushort LandClaimCenterTilePosition { get; set; }

        [SyncToClient]
        public byte LandClaimTier { get; set; }

        public IProtoObjectLandClaim ProtoObjectLandClaim =>
            ProtoObjectLandClaimHelper.GetProtoByTier(this.LandClaimTier);

        public void SetupAreaProperties(LandClaimAreaPrivateState privateState)
        {
            var structure = privateState.ServerLandClaimWorldObject;
            var protoObjectLandClaim = (IProtoObjectLandClaim)structure.ProtoStaticWorldObject;

            var tilePosition = LandClaimSystem.SharedCalculateLandClaimObjectCenterTilePosition(structure);
            this.LandClaimCenterTilePosition = tilePosition;
            this.LandClaimTier = protoObjectLandClaim.LandClaimTier;
        }
    }
}