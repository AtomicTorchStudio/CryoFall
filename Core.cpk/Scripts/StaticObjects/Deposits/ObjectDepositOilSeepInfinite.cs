namespace AtomicTorch.CBND.CoreMod.StaticObjects.Deposits
{
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;

    /// <summary>
    /// Special version of oil seep - doesn't decay over time, doesn't take damage.
    /// Use in public towns.
    /// </summary>
    public class ObjectDepositOilSeepInfinite : ObjectDepositOilSeep
    {
        // no decay
        public override double LifetimeTotalDurationSeconds => 0;

        public override string Name => "Infinite oil seep";

        public override float StructurePointsMax => 0; // non-damageable

        public override void ServerOnExtractorDestroyedForDeposit(IStaticWorldObject objectDeposit)
        {
            // don't explode
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            obstacleBlockDamageCoef = 0;
            damageApplied = 0; // no damage
            return false;      // no hit
        }
    }
}