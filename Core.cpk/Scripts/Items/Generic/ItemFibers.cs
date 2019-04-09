namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemFibers : ProtoItemGeneric
    {
        public override string Description =>
            "Natural or synthetic fibers. Useful to create rope or thread. Could be obtained by gathering grass or certain other plants.";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Fibers";
    }
}