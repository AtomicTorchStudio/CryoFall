namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI;

    public abstract class BaseTechGroupRequirementGroupUnlocked
        : BaseTechGroupRequirement
    {
        // {0} is the tech group name, {1} is the progress number
        public const string DescriptionFormat = "Requires {0} with {1}% of researched technologies.";

        protected BaseTechGroupRequirementGroupUnlocked(double completion, TechGroup techGroup)
        {
            if (completion <= 0
                || completion > 1)
            {
                throw new ArgumentException(
                    "The completion argument must be >0 and <=1",
                    nameof(completion));
            }

            this.GroupNodesUnlockedPercent = completion;
            this.Group = techGroup;
        }

        public TechGroup Group { get; }

        /// <summary>
        /// Gets percent (value from 0 to 1) determining how many nodes must be unlocked at the group to satisfy this requirement.
        /// </summary>
        public double GroupNodesUnlockedPercent { get; }

        protected override bool IsSatisfied(CharacterContext context, out string errorMessage)
        {
            if (!context.Technologies.SharedIsGroupUnlocked(
                    this.Group,
                    this.GroupNodesUnlockedPercent))
            {
                errorMessage = string.Format(DescriptionFormat,
                                             this.Group.NameWithTierName,
                                             (int)Math.Round(this.GroupNodesUnlockedPercent * 100,
                                                             MidpointRounding.AwayFromZero));
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}