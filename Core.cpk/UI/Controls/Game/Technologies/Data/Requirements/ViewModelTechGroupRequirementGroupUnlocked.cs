namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies;

    public class ViewModelTechGroupRequirementGroupUnlocked : BaseViewModelTechGroupRequirement
    {
        private readonly BaseTechGroupRequirementGroupUnlocked requirement;

        public ViewModelTechGroupRequirementGroupUnlocked(BaseTechGroupRequirementGroupUnlocked requirement)
            : base(requirement)
        {
            this.requirement = requirement;
            ClientComponentTechnologiesWatcher.TechNodesChanged += this.TechNodesChangedHandler;
        }

        public double RequiredGroupRequiredNodesUnlockedPercent => this.requirement.GroupNodesUnlockedPercent;

        public string RequiredGroupTitle => this.requirement.Group.Name;

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            ClientComponentTechnologiesWatcher.TechNodesChanged -= this.TechNodesChangedHandler;
        }

        private void TechNodesChangedHandler()
        {
            this.RefreshIsSatisfied();
        }
    }
}