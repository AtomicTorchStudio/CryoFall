namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public class ItemRespirator : ProtoItemEquipmentHead
    {
        public override string Description =>
            "Basic respirator design that can filter out radioactive particles and harmful gases. Doesn't offer any other protection.";

        public override ushort DurabilityMax => 500;

        public override bool IsHairVisible => true;

        public override string Name => "Respirator";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.0,
                kinetic: 0.0,
                heat: 0.0,
                cold: 0.0,
                chemical: 0.0,
                electrical: 0.0,
                radiation: 0.0,
                psi: 0.0);

            // normal value override, we don't want it to be affected by armor multiplier later
            defense.Radiation = 0.4 / defense.Multiplier;
            defense.Chemical = 0.2 / defense.Multiplier;
        }
    }
}