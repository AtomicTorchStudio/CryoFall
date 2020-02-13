namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class LandClaimAreaPrivateState : BasePrivateState
    {
        /// <summary>
        /// Determines whether the land claim was damaged to 0 HP by players.
        /// Reset when the land claim is repaired at least a bit or when the destroy is started due to the decay.
        /// </summary>
        public bool IsDestroyedByPlayers { get; set; }

        [SyncToClient]
        public string LandClaimFounder { get; set; }

        [SyncToClient]
        public NetworkSyncList<string> LandOwners { get; set; }

        public IStaticWorldObject ServerLandClaimWorldObject { get; set; }
    }
}