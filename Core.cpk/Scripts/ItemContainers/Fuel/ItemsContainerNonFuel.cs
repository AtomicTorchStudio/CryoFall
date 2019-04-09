namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerNonFuel : ProtoItemsContainer
    {
        public override bool CanAddItem(CanAddItemContext context)
        {
            return !(context.Item.ProtoItem is IProtoItemFuelSolid);
        }
    }
}