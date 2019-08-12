namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerAnyLiquidContainer : ProtoItemsContainer
    {
        public override bool CanAddItem(CanAddItemContext context)
        {
            var protoItem = context.Item.ProtoItem;
            return protoItem is IProtoItemLiquidStorage
                   || protoItem is ItemCanisterEmpty
                   || protoItem is ItemBottleEmpty;
        }
    }
}