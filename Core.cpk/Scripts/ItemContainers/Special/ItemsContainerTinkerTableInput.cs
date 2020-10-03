namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerTinkerTableInput : ProtoItemsContainer
    {
        public override bool CanAddItem(CanAddItemContext context)
        {
            var character = context.ByCharacter;
            if (character is null)
            {
                return true;
            }

            return context.Item.ProtoItem is IProtoItemWithDurability protoItemWithDurability
                   && protoItemWithDurability.DurabilityMax > 0
                   && protoItemWithDurability.IsRepairable;
        }
    }
}