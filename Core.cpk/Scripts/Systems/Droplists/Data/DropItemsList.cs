﻿namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
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
        private readonly List<ValueWithWeight<Entry>> entries
            = new();

        private ArrayWithWeights<Entry> frozenEntries;

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
        /// <param name="condition">(optional) Special condition.</param>
        public DropItemsList Add<TProtoItem>(
            ushort? count = null,
            ushort countRandom = 0,
            double weight = 1,
            double probability = 1,
            DropItemConditionDelegate condition = null)
            where TProtoItem : class, IProtoItem, new()
        {
            var protoItem = Api.GetProtoEntity<TProtoItem>();
            return this.Add(protoItem,
                            count: count // use default count when provided
                                   ?? (countRandom > 0
                                           ? (ushort)0 // use zero count when random amount is provided
                                           : (ushort)1), // spawn one item if default count is not provided and random is zero
                            countRandom,
                            weight,
                            probability,
                            condition);
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
                                 new Entry(dropItem,
                                           condition,
                                           CreateRegularProbabiltyRollFunc(probability)),
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
                                 new Entry(nestedList,
                                           condition,
                                           CreateRegularProbabiltyRollFunc(probability)),
                                 weight));
            return this;
        }

        public DropItemsList Add(
            IReadOnlyDropItemsListPreset preset,
            double weight = 1,
            DropItemConditionDelegate condition = null)
        {
            var nestedList = preset.DropItemsList;
            if (nestedList == this)
            {
                throw new ArgumentException("Other droplist passed to this method is the same as this droplist",
                                            nameof(nestedList));
            }

            if (nestedList.ContainsDroplist(this))
            {
                throw new Exception("Recursive reference between droplists found!");
            }

            this.entries.Add(new ValueWithWeight<Entry>(
                                 new Entry(nestedList,
                                           condition,
                                           preset.ProbabilityRollFunction),
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
                if (includedList is not null
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
                if (entry.Value.EntryNestedList is not null)
                {
                    foreach (var item in entry.Value.EntryNestedList.EnumerateAllItems())
                    {
                        yield return item;
                    }
                }

                if (entry.Value.EntryItem is not null)
                {
                    yield return entry.Value.EntryItem.ProtoItem;
                }
            }
        }

        public CreateItemResult TryDropToCharacter(
            ICharacter character,
            DropItemContext context,
            double probabilityMultiplier,
            bool sendNoFreeSpaceNotification = true)
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
            double probabilityMultiplier,
            bool sendNotificationWhenDropToGround = true)
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
            double probabilityMultiplier)
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
            double probabilityMultiplier)
        {
            return ServerDroplistHelper.TryDropToGround(
                this,
                tilePosition,
                probabilityMultiplier,
                context,
                out groundContainer);
        }

        CreateItemResult IReadOnlyDropItemsList.Execute(
            DelegateSpawnDropItem delegateSpawnDropItem,
            DropItemContext dropItemContext,
            double probabilityMultiplier)
        {
            return this.ExecuteInternal(delegateSpawnDropItem,
                                        dropItemContext,
                                        probabilityMultiplier);
        }

        private static DropItemRollFunctionDelegate CreateRegularProbabiltyRollFunc(double probability)
        {
            if (probability is <= 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(probability),
                                                      "Probability must be larger than zero and less or greater to 1");
            }

            return Roll;

            bool Roll(DropItemContext context, double probabilityMultiplier, out double resultProbability)
            {
                resultProbability = probability * probabilityMultiplier;
                return RandomHelper.RollWithProbability(resultProbability);
            }
        }

        private static void ExecuteEntry(
            Entry entry,
            DropItemContext dropItemContext,
            out CreateItemResult createItemResult,
            double probabilityMultiplier,
            DelegateSpawnDropItem delegateSpawnDropItem)
        {
            if (entry.Condition is not null)
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

            if (!entry.RollFunction(dropItemContext,
                                    probabilityMultiplier,
                                    out var entryResultProbability))
            {
                Log("Nested entry not rolled - its result probability is " + entryResultProbability.ToString("F3"));
                createItemResult = null;
                return;
            }

            if (entry.EntryNestedList is null)
            {
                createItemResult = ExecuteSpawnItemInternal(entry.EntryItem,
                                                            delegateSpawnDropItem,
                                                            probability: entryResultProbability);
            }
            else
            {
                Log("Nested droplist rolled - its result probability is " + entryResultProbability.ToString("F3"));
                // The code below is correct.
                // For example, if entryResultProbability is 0.05 (1/20th chance) the nested droplist
                // still should provide at least one normal output hence the probabilityMultiplier must be at least 1.0.
                // However, if the probability goes above 1.0 due to a multiplier,
                // this multiplier must propagate to the nested droplist as is.
                var nestedListProbabilityMultiplier = Math.Max(1, entryResultProbability);

                createItemResult = ((DropItemsList)entry.EntryNestedList)
                    .ExecuteInternal(delegateSpawnDropItem,
                                     dropItemContext,
                                     probabilityMultiplier: nestedListProbabilityMultiplier);
            }
        }

        private static CreateItemResult ExecuteSpawnItemInternal(
            DropItem dropItem,
            DelegateSpawnDropItem delegateSpawnDropItem,
            double probability)
        {
            var countToSpawn = dropItem.Count
                               + RandomHelper.Next(minValue: 0,
                                                   maxValueExclusive: dropItem.CountRandom + 1);
            if (countToSpawn <= 0)
            {
                // nothing to spawn this time
                Log(string.Format(
                        "Droplist spawn item ({0}) rolled with result probability {1:F3} but its amount is zero",
                        dropItem.ProtoItem.ShortId,
                        probability));
                return new CreateItemResult() { IsEverythingCreated = true };
            }

            Log($"Droplist spawn item ({dropItem.ProtoItem.ShortId}) rolled with result probability {probability:F3}");

            if (probability > 1)
            {
                countToSpawn = (int)Math.Round(countToSpawn * probability,
                                               MidpointRounding.AwayFromZero);
            }

            if (countToSpawn <= 0)
            {
                return new CreateItemResult() { IsEverythingCreated = true };
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (countToSpawn > ushort.MaxValue)
            {
                countToSpawn = ushort.MaxValue;
            }

            return delegateSpawnDropItem(dropItem.ProtoItem, (ushort)countToSpawn);
        }

        [Conditional("LOGGING")]
        private static void Log(string message)
        {
            Api.Logger.Dev(message);
        }

        private CreateItemResult ExecuteInternal(
            DelegateSpawnDropItem delegateSpawnDropItem,
            DropItemContext dropItemContext,
            double probabilityMultiplier)
        {
            this.Freeze();

            var result = new CreateItemResult() { IsEverythingCreated = true };
            using var selectedEntries = Api.Shared.GetTempList<Entry>();
            this.SelectRandomEntries(selectedEntries, dropItemContext);

            Log($"Starting rolling a droplist with probability multiplier x{probabilityMultiplier:F3}");

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

        private void Freeze()
        {
            this.frozenEntries ??= new ArrayWithWeights<Entry>(this.entries);
        }

        private void SelectRandomEntries(ITempList<Entry> result, DropItemContext dropItemContext)
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
                // simply return a single random selected entry that is satisfying the condition
                Entry entry = default;
                for (var attempt = 0; attempt < 100; attempt++)
                {
                    entry = this.frozenEntries.GetSingleRandomElement();
                    if (entry.Condition is null)
                    {
                        break;
                    }

                    try
                    {
                        if (entry.Condition(dropItemContext))
                        {
                            break;
                        }

                        // condition doesn't match, select another entry
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Api.Logger.Exception(ex, "Exception during checking condition for droplist item " + entry);
                        break;
                    }
                }

                if (!entry.Equals(default))
                {
                    result.Add(entry);
                }

                return;
            }

            // need to select multiple random entries, filter out those that pass the condition
            using var tempAvailableEntries = Api.Shared.GetTempList<ValueWithWeight<Entry>>();
            var availableEntries = tempAvailableEntries.AsList();
            this.frozenEntries.CopyToList(availableEntries);

            for (var index = 0; index < availableEntries.Count; index++)
            {
                var v = availableEntries[index];
                if (v.Value.Condition is not null
                    && !v.Value.Condition(dropItemContext))
                {
                    availableEntries.RemoveAt(index);
                    index--;
                }
            }

            if (availableEntries.Count == 0)
            {
                return;
            }

            SelectRandomElements(availableEntries, result.AsList(), countToSelect);

            static void SelectRandomElements(
                List<ValueWithWeight<Entry>> items,
                List<Entry> result,
                int countToSelect)
            {
                if (countToSelect >= items.Count)
                {
                    // simply return all elements
                    foreach (var item in items)
                    {
                        result.Add(item.Value);
                    }

                    return;
                }

                var currentTotalWeight = items.Sum(e => e.Weight);
                var pickedIndices = new List<ushort>(capacity: countToSelect);

                for (var selection = 0; selection < countToSelect; selection++)
                {
                    // take some random value from 0.0 (inclusive) to 1.0 (exclusive)
                    var value = RandomHelper.NextDouble();
                    // normalize it to current total weight
                    value *= currentTotalWeight;
                    var accumulator = 0.0;

                    for (ushort index = 0; index < items.Count; index++)
                    {
                        if (pickedIndices.Contains(index))
                        {
                            // this item is already picked - ignore it
                            continue;
                        }

                        var item = items[index];
                        accumulator += item.Weight;
                        if (accumulator < value)
                        {
                            continue;
                        }

                        // the item is picked
                        result.Add(item.Value);
                        pickedIndices.Add(index);
                        currentTotalWeight -= item.Weight;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// This struct can container either an Item or an Nested List.
        /// </summary>
        private readonly struct Entry
        {
            public readonly DropItemConditionDelegate Condition;

            public readonly DropItem EntryItem;

            public readonly IReadOnlyDropItemsList EntryNestedList;

            public readonly DropItemRollFunctionDelegate RollFunction;

            public Entry(
                DropItemConditionDelegate condition,
                DropItemRollFunctionDelegate rollFunction)
                : this()
            {
                this.Condition = condition;
                this.RollFunction = rollFunction;
            }

            public Entry(
                IReadOnlyDropItemsList entryNestedList,
                DropItemConditionDelegate condition,
                DropItemRollFunctionDelegate rollFunction)
                : this(condition, rollFunction)
            {
                this.EntryNestedList = entryNestedList;
            }

            public Entry(
                DropItem dropEntryItem,
                DropItemConditionDelegate condition,
                DropItemRollFunctionDelegate rollFunction)
                : this(condition, rollFunction)
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
                                     nameof(this.RollFunction),
                                     this.RollFunction,
                                     nameof(this.Condition),
                                     this.Condition);
            }
        }
    }
}