namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemOreLithium : ProtoItemGeneric
    {
        public override string Description =>
            "Lithium salts are extracted from geothermal springs and can be further processed into pure lithium metal.";

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Lithium salts";
    }
}