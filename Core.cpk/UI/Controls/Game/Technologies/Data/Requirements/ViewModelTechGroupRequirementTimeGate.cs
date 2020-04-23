namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Technologies;

    public class ViewModelTechGroupRequirementTimeGate : BaseViewModelTechGroupRequirement
    {
        private readonly TechGroupRequirementTimeGate requirement;

        public ViewModelTechGroupRequirementTimeGate(TechGroupRequirementTimeGate requirement)
            : base(requirement)
        {
            this.requirement = requirement;
            this.RefreshIsSatisfied();

            this.RefreshByTimer();
        }

        public string DurationRemainsText
            => this.IsSatisfied
                   ? CoreStrings.TechGroupTooltip_TimeGate_AvailableNow
                   : string.Format(CoreStrings.TechGroupTooltip_TimeGate_Format,
                                   ClientTimeFormatHelper.FormatTimeDuration(this.requirement.CalculateTimeRemains()));

        public string DurationText
            => ClientTimeFormatHelper.FormatTimeDuration(this.requirement.DurationSeconds);

        private void RefreshByTimer()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.RefreshIsSatisfied();
            this.NotifyPropertyChanged(nameof(this.DurationRemainsText));

            ClientTimersSystem.AddAction(delaySeconds: 1,
                                         this.RefreshByTimer);
        }
    }
}