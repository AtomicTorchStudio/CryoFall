namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RequirementUseItem : QuestRequirementWithList<IProtoItem>
    {
        public const string DescriptionFormat = "Use: {0}";

        public RequirementUseItem(
            IReadOnlyList<IProtoItem> list,
            ushort count,
            string description)
            : base(list,
                   count,
                   description)
        {
        }

        public override bool IsReversible => false;

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.ListNames);

        public static RequirementUseItem Require<TProtoItem>(
            ushort count = 1,
            string description = null)
            where TProtoItem : class, IProtoItem
        {
            var list = Api.FindProtoEntities<TProtoItem>();
            return new RequirementUseItem(list, count, description);
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                ServerItemUseObserver.ItemUsed += this.ItemUsedHandler;
            }
            else
            {
                ServerItemUseObserver.ItemUsed -= this.ItemUsedHandler;
            }
        }

        private void ItemUsedHandler(ICharacter character, IItem item)
        {
            var context = this.GetActiveContext(character, out var state);
            if (context == null)
            {
                return;
            }

            if (!this.List.Contains(item.ProtoItem))
            {
                return;
            }

            state.SetCountCurrent(state.CountCurrent + 1, this.RequiredCount);
            context.Refresh();
        }
    }
}