namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.GameApi.Scripting;

    [SuppressMessage("ReSharper", "RedundantExplicitArraySize")]
    public static class TechConstants
    {
        public const double LearningPointsPriceBase = 10;

        public const TechTier MaxTier = TechTier.Tier5;

        public const TechTier MinTier = TechTier.Tier1;

        /// <summary>
        /// This constant defines final value for LP gain multiplier at maximum skill level.
        /// This multiplier will fall linearly from 1 at level 0 to the maximum value specified here.
        /// Effectively it reduces LP gain as player raises a particular skill level.
        /// </summary>
        public const double SkillLearningPointMultiplierAtMaximumLevel = 0.33;

        public static readonly double ServerLearningPointsGainMultiplier;

        public static readonly double ServerSkillExperienceGainMultiplier;

        public static readonly double ServerSkillExperienceToLearningPointsConversionMultiplier;

        public static readonly double[] TierGroupPriceMultiplier =
            new double[(byte)MaxTier]
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
                20.0
            };

        public static readonly double[] TierNodePriceMultiplier =
            new double[(byte)MaxTier]
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
                5.0
            };

        // This rate determines the LP rate between skill experience to learning points.
        // Example: with 0.01 conversion rate 100 EXP will result in 1 LP gained.
        // However, it's affected by LearningPointsGainMultiplier which is configured via server rates config.

        static TechConstants()
        {
            if (Api.IsClient)
            {
                return;
            }

            ServerLearningPointsGainMultiplier = ServerRates.Get(
                "LearningPointsGainMultiplier",
                defaultValue: Api.IsServer && SharedLocalServerHelper.IsLocalServer
                                  ? 2.0
                                  : 1.0,
                @"This rate determines the learning points rate
                  from skills experience and neural enhancer consumable item.");

            ServerSkillExperienceGainMultiplier = ServerRates.Get(
                "SkillExperienceGainMultiplier",
                defaultValue: Api.IsServer && SharedLocalServerHelper.IsLocalServer
                                  ? 2.0
                                  : 1.0,
                @"This rate determines the skill experience gain multiplier.                
                If you want to make faster or slower skill progression (and so faster LP gain as well)
                you can modify this multiplier to a higher value.");

            ServerSkillExperienceToLearningPointsConversionMultiplier = 0.01 * ServerLearningPointsGainMultiplier;

            {
                var key = "PvP.TimeGating";
                var defaultValue = "24,72,120,168,216,216";

                var description =
                    @"This rate determines the time-gating values for Tier 3-5 technologies on PvP servers.
                  Please configure a sequence in hours for Tier 3-5 technologies in the following format:
                  T3 basic, T3 specialized, T4 basic, T4 advanced, T5 basic, T5 advanced
                  If you want to disable time-gating completely please use: 0,0,0,0,0,0";

                var currentValue = ServerRates.Get(key, defaultValue, description);

                try
                {
                    ParseTimeGating(currentValue);
                }
                catch
                {
                    Api.Logger.Error($"Incorrect format for server rate: {key} current value {currentValue}");
                    ServerRates.Reset(key, defaultValue, description);
                    currentValue = defaultValue;
                    ParseTimeGating(currentValue);
                }

                static void ParseTimeGating(string str)
                {
                    var split = str.Split(',');
                    if (split.Length != 6)
                    {
                        throw new FormatException();
                    }

                    var durationT3Basic = int.Parse(split[0]);
                    var durationT3Specialized = int.Parse(split[1]);
                    var durationT4Basic = int.Parse(split[2]);
                    var durationT4Specialized = int.Parse(split[3]);
                    var durationT5Basic = int.Parse(split[4]);
                    var durationT5Specialized = int.Parse(split[5]);

                    PvpTechTimeGameTier3Basic = durationT3Basic * 60 * 60;
                    PvpTechTimeGameTier3Specialized = durationT3Specialized * 60 * 60;
                    PvpTechTimeGameTier4Basic = durationT4Basic * 60 * 60;
                    PvpTechTimeGameTier4Specialized = durationT4Specialized * 60 * 60;
                    PvpTechTimeGameTier5Basic = durationT5Basic * 60 * 60;
                    PvpTechTimeGameTier5Specialized = durationT5Specialized * 60 * 60;
                }
            }
        }

        public static event Action ClientLearningPointsGainMultiplierReceived;

        public static event Action ClientPvpTechTimeGameReceivedHandler;

        public static double ClientLearningPointsGainMultiplier { get; private set; }

        // Please note that the time gate is always specified in seconds.
        public static double PvpTechTimeGameTier3Basic { get; private set; }

        public static double PvpTechTimeGameTier3Specialized { get; private set; }

        public static double PvpTechTimeGameTier4Basic { get; private set; }

        public static double PvpTechTimeGameTier4Specialized { get; private set; }

        public static double PvpTechTimeGameTier5Basic { get; private set; }

        public static double PvpTechTimeGameTier5Specialized { get; private set; }

        public static void ClientSetLearningPointsGainMultiplier(double rate)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (ClientLearningPointsGainMultiplier == rate)
            {
                return;
            }

            ClientLearningPointsGainMultiplier = rate;
            Api.SafeInvoke(ClientLearningPointsGainMultiplierReceived);
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public static void ClientSetPvpTechTimeGame(
            double timeGameTier3Basic,
            double timeGameTier3Specialized,
            double timeGameTier4Basic,
            double timeGameTier4Specialized,
            double timeGameTier5Basic,
            double timeGameTier5Specialized)
        {
            if (PvpTechTimeGameTier3Basic == timeGameTier3Basic
                && PvpTechTimeGameTier3Specialized == timeGameTier3Specialized
                && PvpTechTimeGameTier4Basic == timeGameTier4Basic
                && PvpTechTimeGameTier4Specialized == timeGameTier4Specialized
                && PvpTechTimeGameTier5Basic == timeGameTier5Basic
                && PvpTechTimeGameTier5Specialized == timeGameTier5Specialized)
            {
                return;
            }

            PvpTechTimeGameTier3Basic = timeGameTier3Basic;
            PvpTechTimeGameTier3Specialized = timeGameTier3Specialized;
            PvpTechTimeGameTier4Basic = timeGameTier4Basic;
            PvpTechTimeGameTier4Specialized = timeGameTier4Specialized;
            PvpTechTimeGameTier5Basic = timeGameTier5Basic;
            PvpTechTimeGameTier5Specialized = timeGameTier5Specialized;

            Api.SafeInvoke(ClientPvpTechTimeGameReceivedHandler);
        }
    }
}