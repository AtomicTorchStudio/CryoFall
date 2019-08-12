namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemSlime : ProtoItemGeneric, IProtoItemOrganic
    {
        public override string Description => "Green slime, probably left from some creature.";

        public override string Name => "Slime";

        ushort IProtoItemOrganic.OrganicValue => 1;
    }
}