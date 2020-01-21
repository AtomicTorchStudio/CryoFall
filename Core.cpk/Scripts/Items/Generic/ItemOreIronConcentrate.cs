namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemOreIronConcentrate : ProtoItemGeneric
    {
        public override string Description => GetProtoEntity<ItemOreIron>().Description;

        public override string Name => "Iron ore concentrate";
    }
}