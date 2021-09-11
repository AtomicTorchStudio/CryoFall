namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemsContainerTinkerTableDisassemblyInput : ProtoItemsContainer
    {
        public override bool CanAddItem(CanAddItemContext context)
        {
            var character = context.ByCharacter;
            if (character is null)
            {
                return true;
            }

            return ItemDisassemblySystem.SharedCanDisassemble(context.Item.ProtoItem);
        }
    }
}