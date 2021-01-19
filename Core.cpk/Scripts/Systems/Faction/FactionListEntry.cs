namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    [NotPersistent]
    public readonly struct FactionListEntry : IRemoteCallParameter
    {
        public readonly string ClanTag;

        public readonly byte FactionLevel;

        public readonly bool IsAcceptingApplications;

        public readonly FactionKind Kind;

        public readonly ushort LeaderboardRank;

        public readonly string LeaderName;

        public readonly ushort MembersNumberCurrent;

        public readonly string PublicDescription;

        public readonly ulong TotalScore;

        public FactionListEntry(
            string clanTag,
            FactionKind kind,
            bool isAcceptingApplications,
            string leaderName,
            string publicDescription,
            ushort membersNumberCurrent,
            byte factionLevel,
            ushort leaderboardRank,
            ulong totalScore)
        {
            this.ClanTag = clanTag;
            this.Kind = kind;
            this.LeaderName = leaderName;
            this.MembersNumberCurrent = membersNumberCurrent;
            this.IsAcceptingApplications = isAcceptingApplications;
            this.PublicDescription = publicDescription;
            this.FactionLevel = factionLevel;
            this.LeaderboardRank = leaderboardRank;
            this.TotalScore = totalScore;
        }
    }
}