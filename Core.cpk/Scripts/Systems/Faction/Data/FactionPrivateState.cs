namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem.Metrics;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class FactionPrivateState : BasePrivateState
    {
        [SyncToClient]
        public NetworkSyncDictionary<FactionMemberRole, FactionMemberAccessRights> AccessRightsBinding
        {
            get;
            private set;
        }

        [SyncToClient]
        public string DescriptionPrivate { get; set; }

        [SyncToClient]
        public string DescriptionPublic { get; set; }

        /// <summary>
        /// A dictionary where key is the faction clantag and the value is the entry containing the faction diplomacy status.
        /// </summary>
        [SyncToClient]
        public NetworkSyncDictionary<string, FactionDiplomacyStatus> FactionDiplomacyStatuses { get; private set; }

        [SyncToClient]
        public NetworkSyncDictionary<string, FactionAllianceRequest> IncomingFactionAllianceRequests
        {
            get;
            private set;
        }

        public IReadOnlyDictionary<ProtoFactionScoreMetric, uint> LeaderboardScoreByMetric { get; set; }

        [SyncToClient]
        public NetworkSyncList<FactionMemberEntry> Members { get; private set; }

        [SyncToClient]
        public NetworkSyncDictionary<FactionMemberRole, FactionOfficerRoleTitle> OfficerRoleTitleBinding
        {
            get;
            private set;
        }

        [SyncToClient]
        public NetworkSyncDictionary<string, FactionAllianceRequest> OutgoingFactionAllianceRequests
        {
            get;
            private set;
        }

        [SyncToClient]
        public NetworkSyncList<BaseFactionEventLogEntry> RecentEventsLog { get; private set; }

        // please note - this instance is not available via faction private state on client
        public List<FactionSystem.FactionApplication> ServerApplicationsList { get; }
            = new();

        public List<BaseFactionEventLogEntry> ServerEventsLog { get; private set; }
            = new();

        // please note - this instance is not available via faction private state on client
        public ILogicObject ServerFactionChatHolder { get; set; }

        // please note - this instance is not available via faction private state on client
        public List<FactionSystem.FactionInvitation> ServerInvitationsList { get; }
            = new();

        /// <summary>
        /// This metric is incremented by the number of the allocated loot piles from the defeated boss.
        /// </summary>
        public ulong ServerMetricBossScore { get; set; }

        /// <summary>
        /// This metric is incremented by the amount of experience (adjusted by a coefficient)
        /// received by players when killing the mobs.
        /// </summary>
        public double ServerMetricHuntingScore { get; set; }

        /// <summary>
        /// A dictionary containing faction leave date for each player character that has left it
        /// (the key is the character ID).
        /// </summary>
        public Dictionary<uint, double> ServerPlayerLeaveDateDictionary { get; set; }
            = new();

        public void Init()
        {
            this.Members ??= new NetworkSyncList<FactionMemberEntry>();
            this.LeaderboardScoreByMetric ??= new Dictionary<ProtoFactionScoreMetric, uint>();
            this.FactionDiplomacyStatuses ??= new NetworkSyncDictionary<string, FactionDiplomacyStatus>();
            this.IncomingFactionAllianceRequests ??= new NetworkSyncDictionary<string, FactionAllianceRequest>();
            this.OutgoingFactionAllianceRequests ??= new NetworkSyncDictionary<string, FactionAllianceRequest>();
            this.RecentEventsLog ??= new NetworkSyncList<BaseFactionEventLogEntry>();

            this.AccessRightsBinding ??= new NetworkSyncDictionary<FactionMemberRole, FactionMemberAccessRights>
            {
                { FactionMemberRole.Leader, FactionMemberAccessRights.Leader }
            };

            var roleTitles
                = this.OfficerRoleTitleBinding
                      ??= new NetworkSyncDictionary<FactionMemberRole, FactionOfficerRoleTitle>();

            if (!roleTitles.ContainsKey(FactionMemberRole.Officer1))
            {
                roleTitles[FactionMemberRole.Officer1] = FactionOfficerRoleTitle.Deputy;
            }

            if (!roleTitles.ContainsKey(FactionMemberRole.Officer2))
            {
                roleTitles[FactionMemberRole.Officer2] = FactionOfficerRoleTitle.Recruiter;
            }

            if (!roleTitles.ContainsKey(FactionMemberRole.Officer3))
            {
                roleTitles[FactionMemberRole.Officer3] = FactionOfficerRoleTitle.Diplomat;
            }
        }
    }
}