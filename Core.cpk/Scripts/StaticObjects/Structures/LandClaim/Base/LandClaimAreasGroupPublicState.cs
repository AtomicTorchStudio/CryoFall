namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaimShield;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class LandClaimAreasGroupPublicState : BasePublicState
    {
        /// <summary>
        /// In case of a faction land claim, this property contains its clan tag.
        /// </summary>
        [SyncToClient]
        public string FactionClanTag { get; private set; }

        /// <summary>
        /// Is the base was founded by a demo player?
        /// In that case a shortened decay duration should apply.
        /// </summary>
        [SyncToClient]
        public bool IsFounderDemoPlayer { get; set; }

        [SyncToClient]
        [TempOnly]
        public double? LastRaidTime { get; set; }

        public ILogicObject ServerFaction { get; private set; }

        /// <summary>
        /// Determines when the shield protection will activate for this base.
        /// </summary>
        [SyncToClient]
        public double ShieldActivationTime { get; set; }

        [SyncToClient]
        public double ShieldEstimatedExpirationTime { get; set; }

        [SyncToClient]
        public ShieldProtectionStatus Status { get; set; }

        public void ServerSetFaction(ILogicObject faction)
        {
            if (this.ServerFaction is not null
                && !ReferenceEquals(this.ServerFaction, faction))
            {
                throw new Exception("This land claim areas group is already owned by a faction");
            }

            this.ServerFaction = faction;
            this.FactionClanTag = FactionSystem.SharedGetClanTag(faction);
        }
    }
}