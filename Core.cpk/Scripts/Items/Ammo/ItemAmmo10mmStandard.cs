namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemAmmo10mmStandard : ProtoItemAmmo, IAmmoCaliber10mm
    {
        public override string Description =>
            "Universal 10mm rounds designed for general use against a variety of possible targets.";

        public override string Name => "10mm standard ammo";

        protected override void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution)
        {
            damageValue = 20;
            armorPiercingCoef = 0;
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