namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerOrganics : ProtoItemsContainer
    {
        public override bool CanAddItem(CanAddItemContext context)
        {
            return context.Item.ProtoItem is IProtoItemOrganic protoItemOrganic
                   && protoItemOrganic.OrganicValue > 0;
        }
    }
}