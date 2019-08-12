namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemMilitaryPants : ProtoItemEquipmentLegs
    {
        public override string Description => GetProtoEntity<ItemMilitaryJacket>().Description;

        public override uint DurabilityMax => 1000;

        public override string Name => "Military pants";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.50,
                kinetic: 0.60,
                heat: 0.25,
                cold: 0.20,
                chemical: 0.30,
                electrical: 0.25,
                radiation: 0.25,
                psi: 0.0);
        }
    }
}