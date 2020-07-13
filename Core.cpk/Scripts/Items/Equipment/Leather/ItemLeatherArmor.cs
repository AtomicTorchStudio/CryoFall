namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemLeatherArmor : ProtoItemEquipmentArmor
    {
        public override string Description =>
            "Leather armor offers good overall protection and is ideal for early stages of development.";

        public override uint DurabilityMax => 800;

        public override ObjectMaterial Material => ObjectMaterial.SoftTissues;

        public override string Name => "Leather armor";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.50,
                kinetic: 0.40,
                explosion: 0.40,
                heat: 0.35,
                cold: 0.30,
                chemical: 0.20,
                radiation: 0.20,
                psi: 0.0);
        }
    }
}