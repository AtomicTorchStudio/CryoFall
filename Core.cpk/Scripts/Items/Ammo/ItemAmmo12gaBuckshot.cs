namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ItemAmmo12gaBuckshot : ProtoItemAmmo, IAmmoCaliber12g
    {
        public override string Description =>
            "Large pellets of buckshot offer good balance between spread and penetrating power. However, usage of black powder as propellant offers less power than nitrocellulose, limiting this ammo's effectiveness.";

        public override string Name => "12-gauge buckshot charge";

        protected override void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution)
        {
            damageValue = 15;
            armorPiercingCoef = 0;
            finalDamageMultiplier = 2.5;
            rangeMax = 7;

            damageDistribution.Set(DamageType.Kinetic, 0.7);
            damageDistribution.Set(DamageType.Impact,  0.3);
        }
    }
}