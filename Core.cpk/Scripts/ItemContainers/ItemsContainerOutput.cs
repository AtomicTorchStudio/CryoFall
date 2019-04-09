namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// Container disallowing player adding of items (player can only take items, not place).
    /// </summary>
    public class ItemsContainerOutput : ProtoItemsContainer
    {
        public delegate void ServerItemCountChangedDelegate(
            IItemsContainer container,
            IItem item,
            ushort oldCount,
            ushort newCount,
            ICharacter character);

        public delegate void ServerItemRemovedDelegate(IItemsContainer container, IItem item, ICharacter character);

        public static event ServerItemCountChangedDelegate ServerItemCountChanged;

        public static event ServerItemRemovedDelegate ServerItemRemoved;

        public override bool CanAddItem(CanAddItemContext context)
        {
            // prohibit adding items to this container by any character
            return context.ByCharacter == null;
        }

        public override void ServerOnItemCountChanged(
            IItemsContainer container,
            IItem item,
            ushort oldCount,
            ushort newCount,
            ICharacter character)
        {
            base.ServerOnItemCountChanged(container, item, oldCount, newCount, character);
            Api.SafeInvoke(() => ServerItemCountChanged?.Invoke(container, item, oldCount, newCount, character));
        }

        public override void ServerOnItemRemoved(
            IItemsContainer container,
            IItem item,
            ICharacter character)
        {
            base.ServerOnItemRemoved(container, item, character);
            Api.SafeInvoke(() => ServerItemRemoved?.Invoke(container, item, character));
        }
    }
}