namespace AtomicTorch.CBND.CoreMod.Items.Equipment.Assault
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemAssaultArmor : ProtoItemEquipmentArmor
    {
        public override string Description =>
            "This special assault armor provides amazing protection against enemy fire without compromising in other areas.";

        public override uint DurabilityMax => 1200;

        public override ObjectMaterial Material => ObjectMaterial.HardTissues;

        public override string Name => "Assault armor";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.65,
                kinetic: 0.75,
                explosion: 0.70,
                heat: 0.30,
                cold: 0.30,
                chemical: 0.40,
                radiation: 0.30,
                psi: 0.0);
        }
    }
}