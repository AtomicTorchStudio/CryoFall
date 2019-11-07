namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.GameApi.Scripting;

    [SuppressMessage("ReSharper", "RedundantExplicitArraySize")]
    public static class TechConstants
    {
        public const double LearningPointsPriceBase = 10;

        public const TechTier MaxTier = TechTier.Tier4;

        public const TechTier MinTier = TechTier.Tier1;

        /// <summary>
        /// This constant defines final value for LP gain multiplier at maximum skill level.
        /// This multiplier will fall linearly from 1 at level 0 to the maximum value specified here.
        /// Effectively it reduces LP gain as player raises a particular skill level.
        /// </summary>
        public const double SkillLearningPointMultiplierAtMaximumLevel = 0.33;

        public static readonly double ServerLearningPointsGainMultiplier;

        public static readonly double SkillExperienceGainMultiplier;

        // This rate determines the LP rate between skill experience to learning points.
        // Example: with 0.01 conversion rate 100 EXP will result in 1 LP gained.
        // However, it's affected by LearningPointsGainMultiplier which is configured via server rates config.
        public static readonly double SkillExperienceToLearningPointsConversionMultiplier;

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

        static TechConstants()
        {
            ServerLearningPointsGainMultiplier = ServerRates.Get(
                "LearningPointsGainMultiplier",
                defaultValue: 1.0,
                @"This rate determines the learning points rate
                  from skills experience and neural enhancer consumable item.");

            SkillExperienceGainMultiplier = ServerRates.Get(
                "SkillExperienceGainMultiplier",
                defaultValue: 1.0,
                @"This rate determines the skill experience gain multiplier.                
                If you want to make faster or slower skill progression (and so faster LP gain as well)
                you can modify this multiplier to a higher value.");

            SkillExperienceToLearningPointsConversionMultiplier = 0.01 * ServerLearningPointsGainMultiplier;
        }

        public static event Action ClientLearningPointsGainMultiplierChanged;

        public static double ClientLearningPointsGainMultiplier { get; private set; }

        public static void ClientSetLearningPointsGainMultiplier(double rate)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (ClientLearningPointsGainMultiplier == rate)
            {
                return;
            }

            ClientLearningPointsGainMultiplier = rate;
            Api.SafeInvoke(ClientLearningPointsGainMultiplierChanged);
        }
    }
}