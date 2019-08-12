namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerCharacterInventory : ProtoItemsContainer
    {
        public override bool CanAddItem(CanAddItemContext context)
        {
            // allow everything
            return true;
        }
    }
}