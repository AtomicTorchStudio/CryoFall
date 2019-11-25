namespace AtomicTorch.CBND.CoreMod.ItemContainers.Vehicles
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerReactorCorePragmium : ProtoItemsContainer
    {
        public override bool CanAddItem(CanAddItemContext context)
        {
            if (context.Item.ProtoItem is ItemReactorCorePragmium)
            {
                return true;
            }

            if (IsServer
                && context.Item.ProtoItem is ItemReactorCoreEmpty)
            {
                // server can place empty cores here
                return true;
            }

            return false;
        }
    }
}