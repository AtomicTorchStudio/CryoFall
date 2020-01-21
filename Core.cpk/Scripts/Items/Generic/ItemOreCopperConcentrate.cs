namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemOreCopperConcentrate : ProtoItemGeneric
    {
        public override string Description => GetProtoEntity<ItemOreCopper>().Description;

        public override string Name => "Copper ore concentrate";
    }
}