namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class LandClaimAreaPrivateState : BasePrivateState
    {
        [SyncToClient]
        public string LandClaimFounder { get; set; }

        [SyncToClient]
        public NetworkSyncList<string> LandOwners { get; set; }

        public IStaticWorldObject ServerLandClaimWorldObject { get; set; }
    }
}