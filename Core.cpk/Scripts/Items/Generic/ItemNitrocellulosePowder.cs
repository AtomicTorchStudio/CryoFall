namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemNitrocellulosePowder : ProtoItemGeneric
    {
        public override string Description =>
            "Nitrocellulose is also known as smokeless powder. All modern firearms use it instead of black powder.";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Nitrocellulose powder";
    }
}