namespace AtomicTorch.CBND.CoreMod.Items.Equipment.Hazmat
{
    public class ItemHazmatSuit : ProtoItemEquipmentFullBody
    {
        public override string Description =>
            "Specially designed suit that protects its user from most types of environmental hazards. Should not be used in combat, as it will quickly lose its integrity.";

        public override uint DurabilityMax => 400;

        public override bool IsHairVisible => false;

        public override bool IsHeadVisible => false;

        public override string Name => "Hazmat suit";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.15,
                kinetic: 0.10,
                heat: 0.50,
                cold: 0.50,
                chemical: 1.0,
                electrical: 1.0,
                radiation: 0.85,
                psi: 0.40);
        }
    }
}