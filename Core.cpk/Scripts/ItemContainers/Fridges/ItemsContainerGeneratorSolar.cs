namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerGeneratorSolar : ItemsContainerDefault
    {
        protected override bool IsValidateContainerInPrivateScope => true;

        public override bool CanAddItem(CanAddItemContext context)
        {
            if (context.ByCharacter == null)
            {
                // server can place here anything
                // (necessary for placing the broken solar panels)
                return true;
            }

            // player can place here only solar panels
            var protoItem = context.Item.ProtoItem;
            return protoItem is IProtoItemSolarPanel;
        }
    }
}