namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemAmmoArrowIron : ProtoItemAmmo, IAmmoArrow
    {
        public override string Description =>
            "Improved iron-tipped arrow. Still not as good as firearms, but very easy to make.";

        public override bool IsReferenceAmmo => false;

        public override string Name => "Iron-tipped arrow";

        protected override void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution)
        {
            damageValue = 16;
            armorPiercingCoef = 0;
            finalDamageMultiplier = 1.5;
            rangeMax = 7;
            damageDistribution.Set(DamageType.Impact, 1);
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.Arrow;
        }
    }
}