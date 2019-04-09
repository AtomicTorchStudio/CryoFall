namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemCement : ProtoItemGeneric
    {
        public override string Description => "Dry cement mix is useful in construction as a binding agent.";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Dry cement mix";
    }
}