namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public class ObjectGeneratorPragmiumReactorInactiveExplosion : ObjectGeneratorPragmiumReactorActiveExplosion
    {
        public override double DamageRadius => 3;

        protected override void PrepareDamageDescriptionCharacters(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            DamageDistribution damageDistribution)
        {
            damageValue = 60;
            armorPiercingCoef = 0.25;
            finalDamageMultiplier = 1;
            damageDistribution.Set(DamageType.Kinetic, 1);
        }

        protected override void PrepareDamageDescriptionStructures(
            out double damageValue,
            out double defencePenetrationCoef)
        {
            // similar to a Modern bomb (also, the range is much smaller than the active reactor's explosion)
            damageValue = 12_000;
            defencePenetrationCoef = 0.5;
        }
    }
}