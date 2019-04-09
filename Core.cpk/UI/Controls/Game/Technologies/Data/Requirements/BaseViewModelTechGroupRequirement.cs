namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public abstract class BaseViewModelTechGroupRequirement : BaseViewModel
    {
        private readonly BaseTechGroupRequirement requirement;

        protected BaseViewModelTechGroupRequirement(BaseTechGroupRequirement requirement)
        {
            this.requirement = requirement;
            this.RefreshIsSatisfied();
        }

        public bool IsSatisfied { get; private set; }

        public Visibility VisibilityIsNotSatisfied { get; private set; } = Visibility.Visible;

        public Visibility VisibilityIsSatisfied { get; private set; } = Visibility.Collapsed;

        public string GetErrorMessage()
        {
            this.requirement.IsSatisfied(Client.Characters.CurrentPlayerCharacter, out var errorMessage);
            return errorMessage;
        }

        protected void RefreshIsSatisfied()
        {
            var isSatisfiedNow = this.requirement.IsSatisfied(Client.Characters.CurrentPlayerCharacter, out _);
            if (isSatisfiedNow == this.IsSatisfied)
            {
                return;
            }

            this.IsSatisfied = isSatisfiedNow;
            this.VisibilityIsSatisfied = isSatisfiedNow ? Visibility.Visible : Visibility.Collapsed;
            this.VisibilityIsNotSatisfied = !isSatisfiedNow ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}