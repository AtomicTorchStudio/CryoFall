namespace AtomicTorch.CBND.CoreMod.Items.Equipment.Assault
{
    public class ItemAssaultHelmet : ProtoItemEquipmentHead
    {
        public override string Description => GetProtoEntity<ItemAssaultJacket>().Description;

        public override ushort DurabilityMax => 1200;

        public override bool IsHairVisible => false;

        public override string Name => "Assault armor helmet";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.65,
                kinetic: 0.75,
                heat: 0.3,
                cold: 0.3,
                chemical: 0.4,
                electrical: 0.4,
                radiation: 0.3,
                psi: 0.0);

            // normal value override, we don't want it to be affected by armor multiplier later
            defense.Psi = 0.35 / defense.Multiplier;
        }
    }
}