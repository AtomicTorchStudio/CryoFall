namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemExplosives : ProtoItemGeneric
    {
        public override string Description =>
            "High explosive material that could be used to create a variety of explosive devices.";

        public override string Name => "Explosives";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;
    }
}