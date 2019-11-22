namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public interface IReadOnlyWeaponHitSparksPreset
    {
        WeaponHitSparksPreset.HitSparksEntry GetForMaterial(ObjectMaterial material);

        void PreloadTextures();
    }
}