namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskUseItem : BasePlayerTaskWithListAndCount<IProtoItem>
    {
        public const string DescriptionFormat = "Use: {0}";

        public TaskUseItem(
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

        public static TaskUseItem Require<TProtoItem>(
            ushort count = 1,
            string description = null)
            where TProtoItem : class, IProtoItem
        {
            var list = Api.FindProtoEntities<TProtoItem>();
            return Require(list, count, description);
        }

        public static TaskUseItem Require<TProtoItem>(
            IReadOnlyList<TProtoItem> list,
            ushort count = 1,
            string description = null)
            where TProtoItem : class, IProtoItem
        {
            return new(list, count, description);
        }

        public override ITextureResource ClientCreateIcon()
        {
            return this.List.Count == 1
                       ? this.List[0].Icon
                       : null;
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
            if (context is null)
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