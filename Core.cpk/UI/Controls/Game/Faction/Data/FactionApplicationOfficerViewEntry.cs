namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public readonly struct FactionApplicationOfficerViewEntry
    {
        private static readonly ActionCommandWithParameter StaticCommandAcceptApplication
            = new(ExecuteCommandAcceptApplication);

        private static readonly ActionCommandWithParameter StaticCommandRejectApplication
            = new(ExecuteCommandRejectApplication);

        private readonly FactionSystem.ClientApplicationEntry entry;

        public FactionApplicationOfficerViewEntry(FactionSystem.ClientApplicationEntry entry)
        {
            this.entry = entry;
        }

        public string ApplicantName => this.entry.ApplicantName;

        public ActionCommandWithParameter CommandAcceptApplication
            => StaticCommandAcceptApplication;

        public ActionCommandWithParameter CommandRejectApplication
            => StaticCommandRejectApplication;

        public ushort LearningPointsAccumulatedTotal => this.entry.LearningPointsAccumulatedTotal;

        private static void ExecuteCommandAcceptApplication(object obj)
        {
            var applicantName = (string)obj;
            FactionSystem.ClientOfficerApplicationAccept(applicantName);
        }

        private static void ExecuteCommandRejectApplication(object obj)
        {
            var applicantName = (string)obj;
            FactionSystem.ClientOfficerApplicationReject(applicantName);
        }
    }
}