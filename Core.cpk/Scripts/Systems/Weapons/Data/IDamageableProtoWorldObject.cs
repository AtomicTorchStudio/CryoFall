namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IDamageableProtoWorldObject : IProtoWorldObject
    {
        /// <summary>
        /// This coefficient is used to decrease the damage done via ray-casting and explosion.
        /// In case of explosion it will simply pass the explosion though if the value is < 1.
        /// </summary>
        double ObstacleBlockDamageCoef { get; }

        bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IWorldObject targetObject,
            double damagePreMultiplier,
            double damagePostMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied);
    }
}