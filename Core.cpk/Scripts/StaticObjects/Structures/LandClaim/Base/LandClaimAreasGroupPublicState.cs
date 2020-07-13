namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using AtomicTorch.CBND.CoreMod.Systems.LandClaimShield;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class LandClaimAreasGroupPublicState : BasePublicState
    {
        /// <summary>
        /// Is the base was founded by a demo player?
        /// In that case a shortened decay duration should apply.
        /// </summary>
        [SyncToClient]
        public bool IsFounderDemoPlayer { get; set; }

        [SyncToClient]
        [TempOnly]
        public double? LastRaidTime { get; set; }

        /// <summary>
        /// Determines when the shield protection will activate for this base.
        /// </summary>
        [SyncToClient]
        public double ShieldActivationTime { get; set; }

        [SyncToClient]
        public double ShieldEstimatedExpirationTime { get; set; }

        [SyncToClient]
        public ShieldProtectionStatus Status { get; set; }
    }
}