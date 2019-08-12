namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data;

    public class TechGroupRequirementLearningPoints : BaseTechGroupRequirement
    {
        public const string DescriptionFormat = "Requires {0} learning points (only {1} available).";

        public TechGroupRequirementLearningPoints(ushort learningPoints)
        {
            this.LearningPoints = learningPoints;
        }

        public ushort LearningPoints { get; }

        public override BaseViewModelTechGroupRequirement CreateViewModel()
        {
            return new ViewModelTechGroupRequirementLearningPoints(this);
        }

        protected override bool IsSatisfied(CharacterContext context, out string errorMessage)
        {
            var availableLearningPoints = context.Technologies.LearningPoints;
            var isSatisfied = availableLearningPoints >= this.LearningPoints;
            if (isSatisfied)
            {
                errorMessage = null;
                return true;
            }

            errorMessage = string.Format(DescriptionFormat,
                                         this.LearningPoints,
                                         availableLearningPoints);
            return false;
        }
    }
}