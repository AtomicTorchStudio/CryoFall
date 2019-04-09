namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public abstract class BaseItemsContainerFor<TProtoItem> : ProtoItemsContainer
        where TProtoItem : IProtoItem
    {
        protected BaseItemsContainerFor()
        {
        }

        public sealed override bool CanAddItem(CanAddItemContext context)
        {
            return context.Item.ProtoItem is TProtoItem;
        }
    }
}