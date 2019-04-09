namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemVialEmpty : ProtoItemGeneric
    {
        public override string Description => "Empty vial, designed to be used with a biomaterial extraction device.";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Empty biomaterial vial";
    }
}