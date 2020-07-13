namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;

    /// <summary>
    /// Basic interface for both grenades for grenade launchers
    /// and for cannon shells (IAmmoCannonShell) such as used in mechs.
    /// </summary>
    public interface IAmmoGrenade : IProtoItemAmmo, IProtoExplosive, IAmmoWithCustomWeaponCacheDamageDescription
    {
        double DamageRadius { get; }

        ExplosionPreset ExplosionPreset { get; }

        double FireRangeMax { get; }
    }
}