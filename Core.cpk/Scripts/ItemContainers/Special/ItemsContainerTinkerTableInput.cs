namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerTinkerTableInput : ProtoItemsContainer
    {
        public override bool CanAddItem(CanAddItemContext context)
        {
            return context.Item.ProtoItem is IProtoItemWithDurablity protoItemWithDurablity
                   && protoItemWithDurablity.IsRepairable;
        }
    }
}