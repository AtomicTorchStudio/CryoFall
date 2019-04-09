namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemAmmo10mmBlank : ProtoItemAmmo, IAmmoCaliber10mm
    {
        public override string Description =>
            "Special 10mm blank ammo. Can be used for military exercises or to scare away intruders.";

        public override bool IsSuppressWeaponSpecialEffect => true;

        public override string Name => "10mm blank ammo";

        protected override void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution)
        {
            damageValue = 0;
            armorPiercingCoef = 0;
            finalDamageMultiplier = 1;
            rangeMax = 0;

            damageDistribution.Set(DamageType.Kinetic, 1);
        }
    }
}