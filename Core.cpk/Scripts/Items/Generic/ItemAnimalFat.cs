namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemAnimalFat : ProtoItemGeneric, IProtoItemOrganic
    {
        public override string Description => "Fat extracted from animals. Could be useful in cooking or in industry.";

        public override string Name => "Animal fat";

        public ushort OrganicValue => 5;
    }
}