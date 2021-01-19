namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class OutputItems : IReadOnlyOutputItems
    {
        private readonly List<OutputItem> items = new();

        public int Count => this.items.Count;

        public IReadOnlyList<OutputItem> Items => this.items;

        public ProtoItemWithCount this[int index] => this.items[index];

        public static CreateItemResult TrySpawnToContainer(
            IReadOnlyOutputItems outputItems,
            IItemsContainer toContainer)
        {
            var itemsService = Api.Server.Items;
            var result = new CreateItemResult() { IsEverythingCreated = true };
            var character = toContainer.Owner as ICharacter;

            foreach (var outputItem in outputItems.Items)
            {
                var count = GenerateCount(outputItem);
                if (count == 0)
                {
                    continue;
                }

                CreateItemResult createItemResult;
                if (character is not null)
                {
                    createItemResult = itemsService.CreateItem(outputItem.ProtoItem,
                                                               character,
                                                               count);
                }
                else
                {
                    createItemResult = itemsService.CreateItem(outputItem.ProtoItem,
                                                               toContainer,
                                                               count);
                }

                result.MergeWith(createItemResult);
            }

            return result;
        }

        /// <summary>
        /// Add item to the output items list.
        /// </summary>
        /// <typeparam name="TProtoItem">Item prototype.</typeparam>
        /// <param name="count">Minimal amount to spawn (if spawning/adding occurs).</param>
        /// <param name="countRandom">
        /// Additional amount to spawn (value selected randomly from 0 to the specified max value
        /// (inclusive) when spawning).
        /// </param>
        /// <param name="probability">Probability of spawn/add at all.</param>
        /// ///
        public OutputItems Add<TProtoItem>(
            ushort count = 1,
            ushort countRandom = 0,
            double probability = 1)
            where TProtoItem : class, IProtoItem, new()
        {
            var protoItem = Api.GetProtoEntity<TProtoItem>();
            return this.Add(protoItem, count, countRandom, probability);
        }

        /// <summary>
        /// Add item to the output items list.
        /// </summary>
        /// <param name="protoItem">Item prototype instance.</param>
        /// <param name="count">Minimal amount to spawn (if spawning/adding occurs).</param>
        /// <param name="countRandom">
        /// Additional amount to spawn (value selected randomly from 0 to the specified max value
        /// (inclusive) when spawning).
        /// </param>
        /// <param name="probability">Probability of spawn/add at all.</param>
        public OutputItems Add(
            IProtoItem protoItem,
            ushort count = 1,
            ushort countRandom = 0,
            double probability = 1)
        {
            this.items.Add(new OutputItem(protoItem, count, countRandom, probability));
            return this;
        }

        public void ApplyRates(byte multiplier)
        {
            if (multiplier == 1)
            {
                return;
            }

            if (multiplier < 1)
            {
                throw new Exception("Rate modifier cannot be < 1");
            }

            for (var index = 0; index < this.items.Count; index++)
            {
                var entry = this.items[index];
                this.items[index] = entry.WithRate(multiplier);
            }
        }

        public IReadOnlyOutputItems AsReadOnly()
        {
            return this;
        }

        public CreateItemResult TrySpawnToContainer(IItemsContainer toContainer)
        {
            return TrySpawnToContainer(this, toContainer);
        }

        private static ushort GenerateCount(
            OutputItem outputItem,
            double probabilityMultiplier = 1.0)
        {
            // perform random check
            if (!RandomHelper.RollWithProbability(outputItem.Probability * probabilityMultiplier))
            {
                // unlucky one! do not spawn this item
                return 0;
            }

            var countRandom = outputItem.CountRandom;
            if (countRandom == 0)
            {
                return outputItem.Count;
            }

            var result = outputItem.Count + RandomHelper.Next(0, countRandom + 1);
            if (result > ushort.MaxValue)
            {
                return ushort.MaxValue;
            }

            return (ushort)result;
        }
    }
}