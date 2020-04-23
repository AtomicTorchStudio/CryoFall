namespace AtomicTorch.CBND.CoreMod.Items.Equipment.Quilted
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemQuiltedCoat : ProtoItemEquipmentChest
    {
        public override string Description =>
            "Thick padded fabric that provides a surprisingly decent level of protection from all types of damage and environmental effects.";

        public override uint DurabilityMax => 800;

        public override ObjectMaterial Material => ObjectMaterial.SoftTissues;

        public override string Name => "Quilted coat";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.45,
                kinetic: 0.35,
                heat: 0.20,
                cold: 0.50,
                chemical: 0.15,
                electrical: 0.20,
                radiation: 0.15,
                psi: 0);
        }
    }
}