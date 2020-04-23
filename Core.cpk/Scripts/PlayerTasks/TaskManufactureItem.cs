namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskManufactureItem : BasePlayerTaskWithListAndCount<IProtoItem>
    {
        public const string DescriptionFormat = "Manufacture: {0}";

        public TaskManufactureItem(
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

        public static TaskManufactureItem Require<TProtoItem>(
            ushort count = 1,
            string description = null)
            where TProtoItem : class, IProtoItem
        {
            var list = Api.FindProtoEntities<TProtoItem>();
            return new TaskManufactureItem(list,
                                           count,
                                           description);
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
                ItemsContainerOutput.ServerItemRemoved += this.ServerItemRemovedHandler;
                ItemsContainerOutput.ServerItemCountChanged += this.ServerItemCountChangedHandler;
                ServerItemUseObserver.ItemUsed += this.ItemUsedHandler;
            }
            else
            {
                ItemsContainerOutput.ServerItemRemoved -= this.ServerItemRemovedHandler;
                ItemsContainerOutput.ServerItemCountChanged -= this.ServerItemCountChangedHandler;
                ServerItemUseObserver.ItemUsed -= this.ItemUsedHandler;
            }
        }

        private static bool IsValidContainer(IItemsContainer container)
        {
            if (container == null)
            {
                return false;
            }

            var containerOwner = container.OwnerAsStaticObject;
            // we're interested only in output containers in the manufacturer objects
            return container.ProtoItemsContainer is ItemsContainerOutput
                   && containerOwner?.ProtoStaticWorldObject is IProtoObjectManufacturer;
        }

        // We consider using an item from the output container of a manufacturer world object
        // to be equivalent of an item manufacturing.
        private void ItemUsedHandler(ICharacter character, IItem item)
        {
            if (!IsValidContainer(item.Container))
            {
                return;
            }

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

        private void ServerItemCountChangedHandler(
            IItemsContainer container,
            IItem item,
            ushort oldcount,
            ushort newcount,
            ICharacter character)
        {
            if (character == null)
            {
                return;
            }

            var removedCount = oldcount - newcount;
            if (removedCount <= 0)
            {
                return;
            }

            if (!IsValidContainer(container))
            {
                return;
            }

            var context = this.GetActiveContext(character, out var state);
            if (context == null)
            {
                return;
            }

            var protoItem = item.ProtoItem;
            if (!this.List.Contains(protoItem))
            {
                return;
            }

            state.SetCountCurrent(state.CountCurrent + removedCount, this.RequiredCount);
            context.Refresh();
        }

        private void ServerItemRemovedHandler(IItemsContainer container, IItem item, ICharacter character)
        {
            if (character == null
                || !IsValidContainer(container))
            {
                return;
            }

            var context = this.GetActiveContext(character, out var state);
            if (context == null)
            {
                return;
            }

            var protoItem = item.ProtoItem;
            if (!this.List.Contains(protoItem))
            {
                return;
            }

            state.SetCountCurrent(state.CountCurrent + item.Count, this.RequiredCount);
            context.Refresh();
        }
    }
}