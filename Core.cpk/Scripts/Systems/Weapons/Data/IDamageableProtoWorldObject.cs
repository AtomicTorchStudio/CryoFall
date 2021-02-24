namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using JetBrains.Annotations;

    public interface IDamageableProtoWorldObject : IProtoWorldObject
    {
        /// <summary>
        /// This coefficient is used to decrease the damage done via ray-casting and explosion.
        /// In case of explosion it will simply pass the explosion though if the value is < 1.
        /// </summary>
        double ObstacleBlockDamageCoef { get; }

        [CanBeNull]
        ReadOnlySoundPreset<ObjectMaterial> OverrideSoundPresetHit { get; }

        /// <summary>
        /// Used to determine whether the object is at least some kind of obstacle for a particular weapon.
        /// It's used to ignore weapon damage (e.g. for fence wall ih case of some ranged weapons).
        /// It's necessarily to determine whether the laser sight beam should stop on this object or must draw further.
        /// It's not used for bombs.
        /// </summary>
        bool SharedIsObstacle(IWorldObject targetObject, IProtoItemWeapon protoWeapon);

        bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IWorldObject targetObject,
            double damagePreMultiplier,
            double damagePostMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied);
    }
}