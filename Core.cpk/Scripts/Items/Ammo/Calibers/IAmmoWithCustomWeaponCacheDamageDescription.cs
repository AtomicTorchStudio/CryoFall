namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public interface IAmmoWithCustomWeaponCacheDamageDescription
    {
        DamageDescription DamageDescriptionForWeaponCache { get; }
    }
}