namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data
{
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.GameEngine.Common.Extensions;

    public readonly struct FactionKindData
    {
        private const string BulletPointPrefix = "\u2022 ";

        public FactionKindData(FactionKind factionKind)
        {
            this.FactionKind = factionKind;
        }

        public string Description => this.FactionKind.GetAttribute<DescriptionTooltipAttribute>()
                                         .TooltipMessage;

        public string DisplaysMembersStatusAndMapLocationLabel
            => this.FactionKind == FactionKind.Public
                   ? null
                   : BulletPointPrefix + CoreStrings.Faction_Description_ShowsOnlineStatusAndMapLocation;

        public FactionKind FactionKind { get; }

        public bool IsEnabled => this.MembersMax > 0;

        public string LeaderboardParticipationLabel
            => this.FactionKind == FactionKind.Public
                   ? null
                   : BulletPointPrefix + CoreStrings.Faction_Leaderboard_FactionParticipates;

        public string MembersListText
            => BulletPointPrefix
               + string.Format(CoreStrings.Faction_MaxMembers_Format,
                               this.MembersMax);

        public ushort MembersMax
            => FactionConstants.SharedGetFactionMembersMax(this.FactionKind);

        public string Title => this.FactionKind.GetDescription();
    }
}