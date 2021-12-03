namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
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
            
            if (!LandClaimSystem.ValidateIsNotUnderRaidblock(context.Container.OwnerAsStaticObject,
                                                             character))
            {
                // don't allow to place anything in tinker table while the area is under raid
                return false;
            }

            return ItemDisassemblySystem.SharedCanDisassemble(context.Item.ProtoItem);
        }
    }
}