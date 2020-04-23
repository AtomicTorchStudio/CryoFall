namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public class DropItemsList : IReadOnlyDropItemsList
    {
        public static readonly double DropListItemsCountMultiplier;

        private readonly List<ValueWithWeight<Entry>> entries
            = new List<ValueWithWeight<Entry>>();

        private ArrayWithWeights<Entry> frozenEntries;

        static DropItemsList()
        {
            DropListItemsCountMultiplier = ServerRates.Get(
                "DropListItemsCountMultiplier",
                defaultValue: 1.0,
                @"This rate determines the item droplist multiplier.                
                  If you want the objects in game (such as trees, bushes, minerals, loot crates in radtowns, etc)
                  to drop more items during gathering or destruction, you need to adjust this value.");
        }

        /// <summary>
        /// Create a drop list which will try to drop only the defined outputs.
        /// </summary>
        public DropItemsList(ushort outputs, ushort outputsRandom = 0)
        {
            this.Outputs = outputs;
            this.OutputsRandom = outputsRandom;

            if (this.Outputs == 0
                && this.OutputsRandom == 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(outputs),
                    $"Both droplist {nameof(outputs)} and {nameof(outputsRandom)} are zero");
            }
        }

        /// <summary>
        /// Create a drop list which will try to drop all the outputs.
        /// </summary>
        public DropItemsList()
            : this(outputs: ushort.MaxValue,
                   outputsRandom: 0)
        {
        }

        public ushort Outputs { get; set; }

        public ushort OutputsRandom { get; set; }

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
        public DropItemsList Add<TProtoItem>(
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
        public DropItemsList Add(
            IProtoItem protoItem,
            ushort count = 1,
            ushort countRandom = 0,
            double weight = 1,
            double probability = 1,
            DropItemConditionDelegate condition = null)
        {
            var dropItem = new DropItem(protoItem, count, countRandom);
            this.entries.Add(new ValueWithWeight<Entry>(
                                 new Entry(dropItem, condition, probability),
                                 weight));
            return this;
        }

        public DropItemsList Add(
            DropItemsList nestedList,
            double weight = 1,
            double probability = 1,
            DropItemConditionDelegate condition = null)
        {
            if (nestedList == this)
            {
                throw new ArgumentException("Other droplist passed to this method is the same as this droplist",
                                            nameof(nestedList));
            }

            if (this.ContainsDroplist(nestedList))
            {
                throw new Exception("Already included dropitems list.");
            }

            if (nestedList.ContainsDroplist(this))
            {
                throw new Exception("Recursive reference between droplists found!");
            }

            this.entries.Add(new ValueWithWeight<Entry>(
                                 new Entry(nestedList, condition, probability),
                                 weight));
            return this;
        }

        public IReadOnlyDropItemsList AsReadOnly()
        {
            this.Freeze();
            return this;
        }

        public void Clear()
        {
            this.entries.Clear();
        }

        public bool ContainsDroplist(IReadOnlyDropItemsList other)
        {
            if (this == other)
            {
                return true;
            }

            foreach (var entry in this.entries)
            {
                var includedList = entry.Value.EntryNestedList;
                if (includedList != null
                    && includedList.ContainsDroplist(other))
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<IProtoItem> EnumerateAllItems()
        {
            this.Freeze();

            foreach (var entry in this.frozenEntries)
            {
                if (entry.Value.EntryNestedList != null)
                {
                    foreach (var item in entry.Value.EntryNestedList.EnumerateAllItems())
                    {
                        yield return item;
                    }
                }

                if (entry.Value.EntryItem != null)
                {
                    yield return entry.Value.EntryItem.ProtoItem;
                }
            }
        }

        public CreateItemResult Execute(
            DelegateSpawnDropItem delegateSpawnDropItem,
            DropItemContext dropItemContext,
            double probabilityMultiplier)
        {
            this.Freeze();

            var result = new CreateItemResult() { IsEverythingCreated = true };
            using var selectedEntries = Api.Shared.GetTempList<Entry>();
            this.SelectRandomEntries(selectedEntries);

            // execute selected entries
            foreach (var entry in selectedEntries.AsList())
            {
                ExecuteEntry(entry,
                             dropItemContext,
                             out var entryResult,
                             probabilityMultiplier,
                             delegateSpawnDropItem);
                result.MergeWith(entryResult);
            }

            return result;
        }

        public CreateItemResult TryDropToCharacter(
            ICharacter character,
            DropItemContext context,
            bool sendNoFreeSpaceNotification = true,
            double probabilityMultiplier = 1.0)
        {
            return ServerDroplistHelper.TryDropToCharacter(
                this,
                character,
                sendNoFreeSpaceNotification,
                probabilityMultiplier,
                context);
        }

        public CreateItemResult TryDropToCharacterOrGround(
            ICharacter character,
            Vector2Ushort tilePosition,
            DropItemContext context,
            out IItemsContainer groundContainer,
            bool sendNotificationWhenDropToGround = true,
            double probabilityMultiplier = 1D)
        {
            return ServerDroplistHelper.TryDropToCharacterOrGround(
                this,
                character,
                tilePosition,
                sendNotificationWhenDropToGround,
                probabilityMultiplier,
                context,
                out groundContainer);
        }

        public CreateItemResult TryDropToContainer(
            IItemsContainer container,
            DropItemContext context,
            double probabilityMultiplier = 1.0)
        {
            return ServerDroplistHelper.TryDropToContainer(
                this,
                container,
                probabilityMultiplier,
                context);
        }

        public CreateItemResult TryDropToGround(
            Vector2Ushort tilePosition,
            DropItemContext context,
            [CanBeNull] out IItemsContainer groundContainer,
            double probabilityMultiplier = 1.0)
        {
            return ServerDroplistHelper.TryDropToGround(
                this,
                tilePosition,
                probabilityMultiplier,
                context,
                out groundContainer);
        }

        private static void ExecuteEntry(
            Entry entry,
            DropItemContext dropItemContext,
            out CreateItemResult createItemResult,
            double probabilityMultiplier,
            DelegateSpawnDropItem delegateSpawnDropItem)
        {
            if (entry.Condition != null)
            {
                try
                {
                    if (!entry.Condition(dropItemContext))
                    {
                        // condition not match
                        createItemResult = null;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Api.Logger.Exception(ex, "Exception during checking condition for droplist item " + entry);
                    createItemResult = null;
                    return;
                }
            }

            probabilityMultiplier *= entry.Probability;
            createItemResult = entry.EntryNestedList != null
                                   ? entry.EntryNestedList.Execute(delegateSpawnDropItem,
                                                                   dropItemContext,
                                                                   probabilityMultiplier)
                                   : ExecuteSpawnItem(entry.EntryItem,
                                                      delegateSpawnDropItem,
                                                      probabilityMultiplier);
        }

        private static CreateItemResult ExecuteSpawnItem(
            DropItem dropItem,
            DelegateSpawnDropItem delegateSpawnDropItem,
            double probability)
        {
            probability *= DropListItemsCountMultiplier;

            if (!RandomHelper.RollWithProbability(probability))
            {
                return new CreateItemResult() { IsEverythingCreated = true };
            }

            var countToSpawn = dropItem.Count
                               + RandomHelper.Next(minValue: 0,
                                                   maxValueExclusive: dropItem.CountRandom + 1);
            if (countToSpawn <= 0)
            {
                // nothing to spawn this time
                return new CreateItemResult() { IsEverythingCreated = true };
            }

            if (probability > 1)
            {
                var multiplier = probability;
                countToSpawn = (int)Math.Round(countToSpawn * multiplier,
                                               MidpointRounding.AwayFromZero);
            }

            if (countToSpawn <= 0)
            {
                return new CreateItemResult() { IsEverythingCreated = true };
            }

            if (countToSpawn > ushort.MaxValue)
            {
                countToSpawn = ushort.MaxValue;
            }

            return delegateSpawnDropItem(dropItem.ProtoItem, (ushort)countToSpawn);
        }

        private void Freeze()
        {
            if (this.frozenEntries == null)
            {
                this.frozenEntries = new ArrayWithWeights<Entry>(this.entries);
            }
        }

        private void SelectRandomEntries(ITempList<Entry> result)
        {
            var countToSelect = this.Outputs + RandomHelper.Next(0, this.OutputsRandom + 1);
            if (countToSelect >= this.frozenEntries.Count)
            {
                // simply return all the entries
                foreach (var entry in this.entries)
                {
                    result.Add(entry.Value);
                }

                return;
            }

            if (countToSelect == 1)
            {
                // simply return a single random selected entry
                result.Add(this.frozenEntries.GetSingleRandomElement());
                return;
            }

            // need to select multiple random entries
            this.frozenEntries.SelectRandomElements(result.AsList(), countToSelect);
        }

        /// <summary>
        /// This struct can container either an Item or an Nested List.
        /// </summary>
        private readonly struct Entry
        {
            public readonly DropItemConditionDelegate Condition;

            public readonly DropItem EntryItem;

            public readonly IReadOnlyDropItemsList EntryNestedList;

            public readonly double Probability;

            public Entry(DropItemConditionDelegate condition, double probability) : this()
            {
                this.Condition = condition;
                this.Probability = probability;

                if (this.Probability <= 0
                    || this.Probability > 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(probability),
                                                          "Probability must be larger than zero and less or greater to 1");
                }
            }

            public Entry(DropItemsList entryNestedList, DropItemConditionDelegate condition, double probability)
                : this(condition, probability)
            {
                this.EntryNestedList = entryNestedList;
            }

            public Entry(DropItem dropEntryItem, DropItemConditionDelegate condition, double probability)
                : this(condition, probability)
            {
                this.EntryItem = dropEntryItem;
            }

            public override string ToString()
            {
                return string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}",
                                     nameof(this.EntryItem),
                                     this.EntryItem,
                                     nameof(this.EntryNestedList),
                                     this.EntryNestedList,
                                     nameof(this.Probability),
                                     this.Probability,
                                     nameof(this.Condition),
                                     this.Condition);
            }
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                Logger.Info($"Server drop list multiplier set to: {DropListItemsCountMultiplier:F2}");
            }
        }
    }
}