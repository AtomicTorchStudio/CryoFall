namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class LandClaimAreaPublicState : BasePublicState
    {
        [SyncToClient]
        public Vector2Ushort LandClaimCenterTilePosition { get; set; }

        [SyncToClient]
        public ushort LandClaimSize { get; set; }

        [SyncToClient]
        public string Title { get; set; }

        public void SetupAreaProperties(LandClaimAreaPrivateState privateState)
        {
            var structure = privateState.ServerLandClaimWorldObject;
            var protoObjectLandClaim = (IProtoObjectLandClaim)structure.ProtoStaticWorldObject;

            var tilePosition = LandClaimSystem.SharedCalculateLandClaimObjectCenterTilePosition(structure);
            this.LandClaimCenterTilePosition = tilePosition;
            this.LandClaimSize = protoObjectLandClaim.LandClaimSize;
        }
    }
}