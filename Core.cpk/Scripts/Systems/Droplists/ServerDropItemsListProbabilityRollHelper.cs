namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using JetBrains.Annotations;

    /// <summary>
    /// Explained in great detail here https://forums.atomictorch.com/index.php?topic=1734
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
        private static readonly Dictionary<ProbabilityRollKey, Dictionary<uint, ushort>> RollHistory;

        static ServerDropItemsListProbabilityRollHelper()
        {
            if (Api.IsClient)
            {
                return;
            }

            var storagePrefix = nameof(ServerDropItemsListProbabilityRollHelper);
            var storageKey = "History";

            if (!Api.Server.Database.TryGet(storagePrefix,
                                            storageKey,
                                            out RollHistory))
            {
                RollHistory = new Dictionary<ProbabilityRollKey, Dictionary<uint, ushort>>();
                Api.Server.Database.Set(storagePrefix,
                                        storageKey,
                                        RollHistory);
            }
        }

        public static bool Roll(double probability, string storageKey, [CanBeNull] ICharacter character)
        {
            if (character is null)
            {
                // no-character roll - use the regular roll algorithm
                return RandomHelper.RollWithProbability(probability);
            }

            var maxAttemptsNumberRaw = Math.Round(1 / probability, MidpointRounding.AwayFromZero);
            if (maxAttemptsNumberRaw is < 10
                    or > ushort.MaxValue)
            {
                return RandomHelper.RollWithProbability(probability);
                //throw new Exception("Incorrect probability. It should be within 1/10th and 1/65535th");
            }

            maxAttemptsNumberRaw = Math.Min(maxAttemptsNumberRaw, ushort.MaxValue);
            var maxAttemptsNumber = (ushort)maxAttemptsNumberRaw;

            var key = new ProbabilityRollKey(storageKey, maxAttemptsNumber);
            if (!RollHistory.TryGetValue(key, out var dictionaryCharacterRollAttempts))
            {
                RollHistory[key] = dictionaryCharacterRollAttempts = new Dictionary<uint, ushort>();
            }

            var characterId = character.Id;

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
        }

        [Serializable]
        private readonly struct ProbabilityRollKey
            : IEquatable<ProbabilityRollKey>
        {
            public readonly ushort MaxAttemptsNumber;

            public readonly string StorageKey;

            public ProbabilityRollKey(
                string storageKey,
                ushort maxAttemptsNumber)
            {
                this.StorageKey = storageKey;
                this.MaxAttemptsNumber = maxAttemptsNumber;
            }

            public bool Equals(ProbabilityRollKey other)
            {
                return this.StorageKey == other.StorageKey
                       && this.MaxAttemptsNumber == other.MaxAttemptsNumber;
            }

            public override bool Equals(object obj)
            {
                return obj is ProbabilityRollKey other
                       && this.Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = this.StorageKey.GetHashCode();
                    hashCode = (hashCode * 397) ^ this.MaxAttemptsNumber;
                    return hashCode;
                }
            }

            public override string ToString()
            {
                return this.StorageKey + " with chance 1/" + this.MaxAttemptsNumber;
            }
        }
    }
}