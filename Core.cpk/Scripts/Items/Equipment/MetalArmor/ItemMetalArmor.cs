namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemMetalArmor : ProtoItemEquipmentArmor
    {
        public override string Description =>
            "Metal armor offers great protection from physical attacks, but barely any protection from environmental hazards.";

        public override uint DurabilityMax => 1000;

        public override ObjectMaterial Material => ObjectMaterial.Metal;

        public override string Name => "Metal armor";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.60,
                kinetic: 0.45,
                explosion: 0.40,
                heat: 0.20,
                cold: 0.10,
                chemical: 0.15,
                radiation: 0.10,
                psi: 0.0);

            // normal value override, we don't want it to be affected by armor multiplier later
            defense.Psi = 0.15 / defense.Multiplier;
        }
    }
}