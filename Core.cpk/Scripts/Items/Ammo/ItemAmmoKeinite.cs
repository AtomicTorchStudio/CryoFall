namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemAmmoKeinite : ProtoItemAmmo, IAmmoKeinite
    {
        public override string Description => "Liquified keinite in small glass capsules. Used for exotic weapons.";

        public override bool IsReferenceAmmo => true;

        public override ushort MaxItemsPerStack => ItemStackSize.Big;

        public override string Name => "Keinite ammo";

        protected override void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution)
        {
            // default values (not used)
            damageValue = 0;
            armorPiercingCoef = 0;
            finalDamageMultiplier = 1.0;
            rangeMax = 0;
        }

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return default;
        }
    }
}