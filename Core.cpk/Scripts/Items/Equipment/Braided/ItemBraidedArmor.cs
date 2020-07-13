namespace AtomicTorch.CBND.CoreMod.Items.Equipment.Braided
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemBraidedArmor : ProtoItemEquipmentArmor
    {
        public override string Description =>
            "Not exactly the fanciest of outfits, but it could protect you in a pinch. Offers better environmental protection than the closest alternatives.";

        public override uint DurabilityMax => 500;

        public override ObjectMaterial Material => ObjectMaterial.SoftTissues;

        public override string Name => "Braided armor";

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(
                impact: 0.40,
                kinetic: 0.30,
                explosion: 0.40,
                heat: 0.20,
                cold: 0.20,
                chemical: 0.10,
                radiation: 0.15,
                psi: 0.0);
        }
    }
}