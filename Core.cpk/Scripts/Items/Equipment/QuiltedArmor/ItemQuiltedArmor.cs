namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemQuiltedArmor : ProtoItemEquipmentArmor
    {
        public override string Description =>
            "Thick padded fabric that provides a surprisingly decent level of protection from all types of damage and environmental effects.";

        public override uint DurabilityMax => 800;

        public override ObjectMaterial Material => ObjectMaterial.SoftTissues;

        public override string Name => "Quilted armor";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.45,
                kinetic: 0.35,
                explosion: 0.40,
                heat: 0.25,
                cold: 0.50,
                chemical: 0.15,
                radiation: 0.15,
                psi: 0.0);
        }
    }
}