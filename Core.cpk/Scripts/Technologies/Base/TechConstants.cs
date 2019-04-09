namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("ReSharper", "RedundantExplicitArraySize")]
    public static class TechConstants
    {
        public const double LearningPointsPriceBase = 10;

        public const TechTier MaxTier = TechTier.Tier4;

        public const TechTier MinTier = TechTier.Tier1;

        /// <summary>
        /// This constant determines the gained experience multiplier.
        /// If you want to make faster skill experience gain (and so faster LP gain)
        /// you can modify this multiplier to a higher value.
        /// </summary>
        public const double SkillExperienceMultiplier = 1.0;

        /// <summary>
        /// This constant determines the conversion rate between skill experience to learning points.
        /// Example: with 0.01 conversion rate 100 EXP will result in 1 LP gained.
        /// </summary>
        public const double SkillExperienceToLearningPointsConversionMultiplier = 0.01;

        /// <summary>
        /// This constant defines final value for LP gain multiplier at maximum skill level.
        /// This multiplier will fall linearly from 1 at level 0 to the maximum value specified here.
        /// Effectively it reduces LP gain as player raises a particular skill level.
        /// </summary>
        public const double SkillLearningPointMultiplierAtMaximumLevel = 0.25;

        public static readonly double[] TierGroupPriceMultiplier =
            new double[(byte)MaxTier]
            {
                // tier 1 (unlocked by default)
                0,
                // tier 2
                10.0,
                // tier 3
                25.0,
                // tier 4
                50.0
            };

        public static readonly double[] TierNodePriceMultiplier =
            new double[(byte)MaxTier]
            {
                // tier 1 (unlocked by default)
                1.0,
                // tier 2
                2.0,
                // tier 3
                5.0,
                // tier 4
                10.0
            };
    }
}