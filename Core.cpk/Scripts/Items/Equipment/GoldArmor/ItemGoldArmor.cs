namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemGoldArmor : ProtoItemEquipmentArmor
    {
        public override string Description => "Luxurious golden attire. Befitting of true rulers.";

        public override uint DurabilityMax => 800;

        public override ObjectMaterial Material => ObjectMaterial.Metal;

        public override string Name => "Gold armor";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.45,
                kinetic: 0.30,
                explosion: 0.35,
                heat: 0.10,
                cold: 0.10,
                chemical: 0.25,
                radiation: 0.25,
                psi: 0.0);
        }
    }
}