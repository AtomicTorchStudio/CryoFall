namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemBlackpowder : ProtoItemGeneric
    {
        public override string Description =>
            "Normal gunpowder, also known as black powder. Can be easily produced from simple ingredients.";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Black powder";
    }
}