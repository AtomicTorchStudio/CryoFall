namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RequirementHaveItem : QuestRequirementWithList<IProtoItem>
    {
        public const string DescriptionFormat = "Collect: {0}";

        private ServerWrappedTriggerTimeInterval serverUpdater;

        private RequirementHaveItem(
            IReadOnlyList<IProtoItem> protoItems,
            ushort count,
            string description,
            bool isReversible)
            : base(protoItems,
                   count,
                   description)
        {
            this.IsReversible = isReversible;
        }

        public override bool IsReversible { get; }

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.ListNames);

        /// <param name="isReversible">If true, item might be removed and requirement will become unsatisfied again</param>
        public static RequirementHaveItem Require<TProtoItem>(
            ushort count,
            string description = null,
            bool isReversible = true)
            where TProtoItem : class, IProtoItem
        {
            var protoItems = Api.FindProtoEntities<TProtoItem>();
            return new RequirementHaveItem(protoItems.ToArray(), count, description, isReversible);
        }

        protected override bool ServerIsSatisfied(ICharacter character, QuestRequirementStateWithCount state)
        {
            var characterContainers = new AggregatedItemsContainers(character, includeEquipmentContainer: true);

            if (this.RequiredCount == 1)
            {
                // only one item is required
                foreach (var protoItem in this.List)
                {
                    if (characterContainers.ContainsItemsOfType(protoItem, requiredCount: 1))
                    {
                        // found at least one item of the required item type
                        state.SetCountCurrent(1, countMax: 1);
                        return true;
                    }
                }

                state.SetCountCurrent(0, countMax: 1);
                return false;
            }

            // more than a single item is required
            var availableCount = 0;
            foreach (var protoItem in this.List)
            {
                availableCount += characterContainers.CountItemsOfType(protoItem);
                if (availableCount >= this.RequiredCount)
                {
                    break;
                }
            }

            state.SetCountCurrent(availableCount, this.RequiredCount);
            return state.CountCurrent >= this.RequiredCount;
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                this.serverUpdater = new ServerWrappedTriggerTimeInterval(
                    this.ServerRefreshAllActiveContexts,
                    TimeSpan.FromSeconds(1),
                    "QuestRequirement.HaveItem");
            }
            else
            {
                this.serverUpdater.Dispose();
                this.serverUpdater = null;
            }
        }
    }
}