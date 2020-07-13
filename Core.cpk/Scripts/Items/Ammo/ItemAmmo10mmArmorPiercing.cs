namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemAmmo10mmArmorPiercing : ProtoItemAmmo, IAmmoCaliber10mm
    {
        public override string Description =>
            "Modification of the 10mm round specifically designed to provide higher armor penetration potential. Ideal against armored targets, but much less effective against biological targets.";

        public override bool IsReferenceAmmo => false;

        public override string Name => "10mm armor-piercing ammo";

        protected override void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution)
        {
            damageValue = 18;
            armorPiercingCoef = 0.5;
            finalDamageMultiplier = 1;
            rangeMax = 10;
            damageDistribution.Set(DamageType.Kinetic, 1);
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.Firearm;
        }
    }
}