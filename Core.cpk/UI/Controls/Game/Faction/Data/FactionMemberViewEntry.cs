namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;

    public readonly struct FactionMemberViewEntry
    {
        private readonly FactionMemberEntry entry;

        public FactionMemberViewEntry(FactionMemberEntry entry, bool isOnlineStatusAvailable, bool isOnline)
        {
            this.entry = entry;
            this.IsOnline = isOnline;
            this.IsOnlineStatusAvailable = isOnlineStatusAvailable;
        }

        public bool IsCurrentPlayerEntry
            => this.Name == ClientCurrentCharacterHelper.Character.Name;

        public bool IsLeader => this.Role == FactionMemberRole.Leader;

        public bool IsOfficer => this.Role != FactionMemberRole.Member;

        public bool IsOfficerExceptLeader => this.IsOfficer && !this.IsLeader;

        public bool IsOnline { get; }

        public bool IsOnlineStatusAvailable { get; }

        public string Name => this.entry.Name;

        public FactionMemberRole Role => this.entry.Role;

        public string RoleTitle => FactionSystem.ClientGetRoleTitle(this.Role);

        public FactionMemberViewEntry WithOnlineStatus(bool isOnline)
        {
            return new(this.entry, this.IsOnlineStatusAvailable, isOnline);
        }
    }
}