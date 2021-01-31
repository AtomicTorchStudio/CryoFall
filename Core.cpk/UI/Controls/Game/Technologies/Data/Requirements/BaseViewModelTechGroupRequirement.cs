namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
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

        protected void RefreshIsSatisfied()
        {
            this.IsSatisfied = this.requirement.IsSatisfied(Client.Characters.CurrentPlayerCharacter, out _);
        }
    }
}