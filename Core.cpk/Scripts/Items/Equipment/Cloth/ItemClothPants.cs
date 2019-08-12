namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemClothPants : ProtoItemEquipmentLegs
    {
        public override string Description => GetProtoEntity<ItemClothShirt>().Description;

        public override uint DurabilityMax => 300;

        public override string Name => "Cloth pants";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.25,
                kinetic: 0.20,
                heat: 0.15,
                cold: 0.15,
                chemical: 0.1,
                electrical: 0.1,
                radiation: 0.1,
                psi: 0);
        }
    }
}