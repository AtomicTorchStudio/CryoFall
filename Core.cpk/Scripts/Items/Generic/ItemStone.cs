namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemStone : ProtoItemGeneric
    {
        public override string Description =>
            "Stone...round and heavy. Useful in construction. Can often be found just lying on the ground or mined in higher quantities.";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Stone";
    }
}