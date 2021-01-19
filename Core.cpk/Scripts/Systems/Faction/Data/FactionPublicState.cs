namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class FactionPublicState : BasePublicState
    {
        [SyncToClient]
        public string ClanTag { get; set; }

        [SyncToClient]
        public FactionEmblem Emblem { get; set; }

        /// <summary>
        /// Officers could configure the faction to stop accepting applications
        /// (in case of private faction) or joining freely (in case of public faction).
        /// </summary>
        [SyncToClient(isAllowClientSideModification: true)]
        public bool IsAcceptingApplications { get; set; } = true;

        /// <summary>
        /// Determines whether the faction is "Open" - anyone can join without submitting an application.
        /// </summary>
        [SyncToClient]
        public FactionKind Kind { get; set; }

        [SyncToClient]
        public ushort LeaderboardRank { get; set; }

        [SyncToClient]
        public string LeaderName { get; set; }

        [SyncToClient]
        public byte Level { get; set; } = 1;

        [SyncToClient]
        public ushort PlayersNumberCurrent { get; set; }

        [SyncToClient]
        public ulong TotalScore { get; set; }
    }
}