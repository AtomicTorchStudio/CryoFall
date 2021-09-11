namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemSalt : ProtoItemGeneric
    {
        public override string Description =>
            "Normal table salt... Can be used in preparation of food. Also known as NaCl to chemists. Has a wide range of applications.";

        public override string Name => "Salt";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;
    }
}