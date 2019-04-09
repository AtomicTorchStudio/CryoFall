namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemMilitaryHelmet : ProtoItemEquipmentHead
    {
        public override string Description => GetProtoEntity<ItemMilitaryJacket>().Description;

        public override ushort DurabilityMax => 1000;

        public override bool IsHairVisible => false;

        public override string Name => "Military helmet";

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

            // normal value override, we don't want it to be affected by armor multiplier later
            defense.Psi = 0.3 / defense.Multiplier;
        }
    }
}