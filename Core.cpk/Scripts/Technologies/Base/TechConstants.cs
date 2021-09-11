namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.GameApi.Scripting;

    [SuppressMessage("ReSharper", "RedundantExplicitArraySize")]
    public static class TechConstants
    {
        public const double LearningPointsPriceBase = 10;

        public const TechTier MaxTier = TechTier.Tier6;

        public const TechTier MinTier = TechTier.Tier1;

        public const TechTier PvPTimeGateStartsFromTier = TechTier.Tier3;

        /// <summary>
        /// This constant defines final value for LP gain multiplier at maximum skill level.
        /// This multiplier will fall linearly from 1 at level 0 to the maximum value specified here.
        /// Effectively it reduces LP gain as player raises a particular skill level.
        /// </summary>
        public const double SkillLearningPointMultiplierAtMaximumLevel = 0.33;

        public static readonly double ServerLearningPointsGainMultiplier;

        public static readonly double ServerSkillExperienceGainMultiplier;

        /// <summary>
        /// Determines the LP rate between skill experience to learning points.
        /// Example: with 0.01 conversion rate 100 EXP will result in 1 LP gained.
        /// Please note: it's affected by LearningPointsGainMultiplier which is configured in server rates config.
        /// </summary>
        public static readonly double ServerSkillExperienceToLearningPointsConversionMultiplier;

        public static readonly double[] TierGroupPriceMultiplier
            = new double[(byte)MaxTier]
            {
                // tier 1 (unlocked by default)
                0,
                // tier 2
                5.0,
                // tier 3
                10.0,
                // tier 4
                15.0,
                // tier 5
                20.0,
                // tier 6
                20.0
            };

        public static readonly double[] TierNodePriceMultiplier
            = new double[(byte)MaxTier]
            {
                // tier 1 (unlocked by default)
                1.0,
                // tier 2
                2.0,
                // tier 3
                3.0,
                // tier 4
                4.0,
                // tier 5
                5.0,
                // tier 6
                5.0
            };

        static TechConstants()
        {
            if (Api.IsClient)
            {
                return;
            }

            ServerLearningPointsGainMultiplier = RateLearningPointsGainMultiplier.SharedValue;
            ServerSkillExperienceGainMultiplier = RateSkillExperienceGainMultiplier.SharedValue;
            ServerSkillExperienceToLearningPointsConversionMultiplier = 0.01 * ServerLearningPointsGainMultiplier;
        }

        public static double ClientLearningPointsGainMultiplier
            => RateLearningPointsGainMultiplier.SharedValue;

        public static PvPTechTimeGateDurations PvPTimeGates
            => RatePvPTimeGates.SharedGetTimeGateDurations();
    }
}