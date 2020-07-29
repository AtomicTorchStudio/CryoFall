namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    /// <summary>
    /// This system provides a special item droplist roll condition
    /// that ensures that probability rolling ends up in a flat distribution of the number of the attempts.
    /// For example, the average and median number of attempts necessary to obtain an item
    /// with 1/1000th probability are exactly 1000 average and 1000 median.
    /// The distribution range is up to x2 more attempts than specified
    /// (e.g. for 1/1000th probability it could take up to 1999 attempts max
    /// but the distribution of number of attempts to obtain an item is completely even).
    /// Here is the test sample app https://github.com/aienabled/RandomDistributionTest
    /// (requires Visual Studio 2019 with desktop development setup for WPF apps).
    /// </summary>
    public static class ServerDropItemsListProbabilityRollHelper
    {
        private static readonly Dictionary<string, Dictionary<uint, ushort>> RollHistory;

        static ServerDropItemsListProbabilityRollHelper()
        {
            if (Api.IsClient)
            {
                return;
            }

            var storagePrefix = nameof(ServerDropItemsListProbabilityRollHelper);
            var storageKey = "Entries";

            if (!Api.Server.Database.TryGet(storagePrefix,
                                            storageKey,
                                            out RollHistory))
            {
                RollHistory = new Dictionary<string, Dictionary<uint, ushort>>();
                Api.Server.Database.Set(storagePrefix,
                                        storageKey,
                                        RollHistory);
            }
        }

        public static DropItemConditionDelegate CreateRollCondition(double probability, string key)
        {
            var maxAttemptsNumberRaw = Math.Round(1 / probability, MidpointRounding.AwayFromZero);
            if (maxAttemptsNumberRaw < 10
                || maxAttemptsNumberRaw > ushort.MaxValue)
            {
                throw new Exception("Incorrect probability. It should be within 1/10th and 1/65535th");
            }

            var maxAttemptsNumber = (ushort)maxAttemptsNumberRaw;

            key += "_1/" + maxAttemptsNumber.ToString("0.#####") + "th";

            if (!RollHistory.TryGetValue(key, out var dictionaryCharacterRollAttempts))
            {
                RollHistory[key]
                    = dictionaryCharacterRollAttempts
                          = new Dictionary<uint, ushort>();
            }

            return context =>
                   {
                       if (!context.HasCharacter)
                       {
                           // no-character roll - use the regular roll algorithm
                           return RandomHelper.RollWithProbability(probability);
                       }

                       var characterId = context.Character.Id;

                       // get the number of roll attempts made
                       if (!dictionaryCharacterRollAttempts.TryGetValue(characterId, out var attemptsMade))
                       {
                           attemptsMade = 0;
                       }

                       if (attemptsMade < ushort.MaxValue)
                       {
                           attemptsMade++;
                       }

                       // Probability compensation mechanism idea:
                       // the more attempts were made, the bigger threshold we will use allowing faster match.
                       var p = 2 * maxAttemptsNumber - attemptsMade;
                       p = Math.Max(1, p);

                       var rolledValue = RandomHelper.NextDouble();
                       var isSuccessfulRoll = rolledValue <= 1.0 / p;

                       dictionaryCharacterRollAttempts[characterId] = isSuccessfulRoll
                                                                          ? (ushort)0
                                                                          : attemptsMade;

                       //Api.Logger.Dev(
                       //    $"{(isSuccessfulRoll ? "Successful" : "Failed")} roll! {attemptsMade} attempts were made for {key}");

                       return isSuccessfulRoll;
                   };
        }
    }
}