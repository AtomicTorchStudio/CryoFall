namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IReadOnlyWeaponHitSparksPreset
    {
        WeaponHitSparksPreset.HitSparksEntry GetForMaterial(ObjectMaterial material);
    }
}