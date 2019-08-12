namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerGeneratorSolar : ItemsContainerDefault
    {
        protected override bool IsValidateContainerInPrivateScope => true;

        public override bool CanAddItem(CanAddItemContext context)
        {
            var protoItem = context.Item.ProtoItem;
            return protoItem is IProtoItemSolarPanel;
        }
    }
}