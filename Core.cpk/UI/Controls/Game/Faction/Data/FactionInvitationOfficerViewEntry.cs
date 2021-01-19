namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public readonly struct FactionInvitationOfficerViewEntry
    {
        private static readonly ActionCommandWithParameter StaticCommandRemoveInvitation
            = new(ExecuteCommandRemoveInvitation);

        private readonly FactionSystem.ClientInvitationEntry entry;

        public FactionInvitationOfficerViewEntry(FactionSystem.ClientInvitationEntry entry)
        {
            this.entry = entry;
        }

        public ActionCommandWithParameter CommandRemoveInvitation
            => StaticCommandRemoveInvitation;

        public string InviteeName => this.entry.InviteeName;

        public string InviterName => this.entry.InviterName;

        private static void ExecuteCommandRemoveInvitation(object obj)
        {
            var inviteeName = (string)obj;
            FactionSystem.ClientOfficerRemoveInvitation(inviteeName);
        }
    }
}