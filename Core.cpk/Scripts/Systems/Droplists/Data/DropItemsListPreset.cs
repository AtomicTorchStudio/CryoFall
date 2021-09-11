namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class DropItemsListPreset : IReadOnlyDropItemsListPreset
    {
        public readonly DropItemsList dropItemsList = new();

        private readonly double probability;

        public DropItemsListPreset(
            ushort outputs,
            double probability,
            bool useGuaranteedProbabilityAlgorithm,
            string storageKey,
            ushort outputsRandom = 0)
        {
            if (probability is <= 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(probability),
                                                      "Probability must be larger than zero and less or greater to 1");
            }

            this.UseGuaranteedProbabilityAlgorithm = useGuaranteedProbabilityAlgorithm;
            this.StorageKey = storageKey;
            this.probability = probability;
            this.dropItemsList.Outputs = outputs;
            this.dropItemsList.OutputsRandom = outputsRandom;
        }

        public IReadOnlyDropItemsList DropItemsList => this.dropItemsList;

        public DropItemRollFunctionDelegate ProbabilityRollFunction => this.Roll;

        public string StorageKey { get; }

        public bool UseGuaranteedProbabilityAlgorithm { get; }

        /// <summary>
        /// Add item to the droplist.
        /// </summary>
        /// <typeparam name="TProtoItem">Item prototype.</typeparam>
        /// <param name="count">Minimal amount to spawn (if spawning/adding occurs).</param>
        /// <param name="countRandom">
        /// Additional amount to spawn (value selected randomly from 0 to the specified max value
        /// (inclusive) when spawning).
        /// </param>
        /// <param name="weight">Probability of spawn/add at all.</param>
        /// ///
        /// <param name="condition">(optional) Special condition.</param>
        public DropItemsListPreset Add<TProtoItem>(
            ushort count = 1,
            ushort countRandom = 0,
            double weight = 1,
            double probability = 1,
            DropItemConditionDelegate condition = null)
            where TProtoItem : class, IProtoItem, new()
        {
            var protoItem = Api.GetProtoEntity<TProtoItem>();
            return this.Add(protoItem, count, countRandom, weight, probability, condition);
        }

        /// <summary>
        /// Add item to the droplist.
        /// </summary>
        /// <param name="protoItem">Item prototype instance.</param>
        /// <param name="count">Minimal amount to spawn (if spawning/adding occurs).</param>
        /// <param name="countRandom">
        /// Additional amount to spawn (value selected randomly from 0 to the specified max value
        /// (inclusive) when spawning).
        /// </param>
        /// <param name="weight">
        /// Weight of this item in the droplist (higher weight in comparison to other entries - higher chance
        /// to spawn).
        /// </param>
        /// <param name="condition">(optional) Special condition.</param>
        public DropItemsListPreset Add(
            IProtoItem protoItem,
            ushort count = 1,
            ushort countRandom = 0,
            double weight = 1,
            double probability = 1,
            DropItemConditionDelegate condition = null)
        {
            this.dropItemsList.Add(protoItem,
                                   count: count,
                                   countRandom: countRandom,
                                   weight: weight,
                                   probability: probability,
                                   condition: condition);
            return this;
        }

        private bool Roll(
            DropItemContext context,
            double probabilityMultiplier,
            out double resultProbability)
        {
            resultProbability = this.probability * probabilityMultiplier;
            resultProbability = Math.Max(resultProbability, 0);

            if (resultProbability <= 0)
            {
                return false;
            }

            if (resultProbability >= 1)
            {
                return true;
            }

            var character = context.HasCharacter
                                ? context.Character
                                : null;

            return ServerDropItemsListProbabilityRollHelper
                .Roll(resultProbability, this.StorageKey, character);
        }
    }
}