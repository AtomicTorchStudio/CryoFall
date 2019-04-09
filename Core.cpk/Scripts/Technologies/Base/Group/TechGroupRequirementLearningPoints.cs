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

        protected override bool IsSatisfied(CharacterContext context, out string error)
        {
            var availableLearningPoints = context.Technologies.LearningPoints;
            var isSatisfied = availableLearningPoints >= this.LearningPoints;
            if (isSatisfied)
            {
                error = null;
                return true;
            }

            error = string.Format(DescriptionFormat,
                                  this.LearningPoints,
                                  availableLearningPoints);
            return false;
        }
    }
}