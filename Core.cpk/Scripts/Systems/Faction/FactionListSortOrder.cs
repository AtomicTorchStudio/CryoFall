namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum FactionListSortOrder : byte
    {
        [Description("Members number")]
        MembersNumber = 0,

        [Description(CoreStrings.Faction_Leaderboard_Rank)]
        LeaderboardRank = 1,

        [Description(CoreStrings.Faction_Level)]
        Level = 2
    }
}