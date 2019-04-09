namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies;

    public class ViewModelTechGroupRequirementLearningPoints : BaseViewModelTechGroupRequirement
    {
        private readonly TechGroupRequirementLearningPoints requirement;

        public ViewModelTechGroupRequirementLearningPoints(TechGroupRequirementLearningPoints requirement)
            : base(requirement)
        {
            this.requirement = requirement;
            this.RefreshIsSatisfied();

            ClientComponentTechnologiesWatcher.LearningPointsChanged += this.LearningPointsChangedHandler;
        }

        public ushort RequiredLearningPoints => this.requirement.LearningPoints;

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            ClientComponentTechnologiesWatcher.LearningPointsChanged -= this.LearningPointsChangedHandler;
        }

        private void LearningPointsChangedHandler()
        {
            this.RefreshIsSatisfied();
        }
    }
}