namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemAmmo12gaPellets : ProtoItemAmmo, IAmmoCaliber12g
    {
        public override string Description =>
            "Pellets spread out covering higher area and are ideal for smaller biological targets, especially against groups of enemies. Highly ineffective against armored targets.";

        public override string Name => "12-gauge pellets charge";

        protected override void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution)
        {
            damageValue = 10;
            armorPiercingCoef = 0;
            finalDamageMultiplier = 5;
            rangeMax = 7;

            damageDistribution.Set(DamageType.Kinetic, 0.8);
            damageDistribution.Set(DamageType.Impact,  0.2);
        }
    }
}