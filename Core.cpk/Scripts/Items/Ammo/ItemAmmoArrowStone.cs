namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemAmmoArrowStone : ProtoItemAmmo, IAmmoArrow
    {
        public override string Description =>
            "Simple stone-tipped arrow. Can't go simpler than that.";

        public override bool IsReferenceAmmo => true;

        public override string Name => "Stone-tipped arrow";

        protected override void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution)
        {
            damageValue = 19;
            armorPiercingCoef = 0;
            finalDamageMultiplier = 1.4;
            rangeMax = 6;
            damageDistribution.Set(DamageType.Impact, 1);
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.Arrow;
        }
    }
}