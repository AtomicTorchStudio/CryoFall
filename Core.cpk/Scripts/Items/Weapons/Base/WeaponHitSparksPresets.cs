namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;

    public static class WeaponHitSparksPresets
    {
        public static readonly IReadOnlyWeaponHitSparksPreset Firearm
            = new WeaponHitSparksPreset()
              .SetDefault(
                  new TextureAtlasResource("FX/HitSparks/HitSparksStone",
                                           columns: 4,
                                           rows: 1,
                                           isTransparent: true))
              .Add(ObjectMaterial.Wood,
                   new TextureAtlasResource("FX/HitSparks/HitSparksWood",
                                            columns: 4,
                                            rows: 1,
                                            isTransparent: true))
              .Add(ObjectMaterial.Metal,
                   new TextureAtlasResource("FX/HitSparks/HitSparksMetal",
                                            columns: 4,
                                            rows: 1,
                                            isTransparent: true),
                   lightColor: Color.FromArgb(0x99, 0xFF, 0xEE, 0XAA))
              .Add(ObjectMaterial.HardTissues,
                   new TextureAtlasResource("FX/HitSparks/HitSparksHardTissue",
                                            columns: 4,
                                            rows: 1,
                                            isTransparent: true))
              .Add(ObjectMaterial.SoftTissues,
                   new TextureAtlasResource("FX/HitSparks/HitSparksSoftTissue",
                                            columns: 4,
                                            rows: 1,
                                            isTransparent: true))
              .Add(ObjectMaterial.Vegetation,
                   new TextureAtlasResource("FX/HitSparks/HitSparksVegetation",
                                            columns: 4,
                                            rows: 1,
                                            isTransparent: true));

        public static readonly IReadOnlyWeaponHitSparksPreset NoWeapon
            = new WeaponHitSparksPreset()
              .SetDefault(
                  new TextureAtlasResource("FX/HitSparks/HitSparksHardTissue",
                                           columns: 4,
                                           rows: 1,
                                           isTransparent: true))
              .Add(ObjectMaterial.Wood,
                   new TextureAtlasResource("FX/HitSparks/HitSparksWood",
                                            columns: 4,
                                            rows: 1,
                                            isTransparent: true))
              .Add(ObjectMaterial.HardTissues,
                   new TextureAtlasResource("FX/HitSparks/HitSparksHardTissue",
                                            columns: 4,
                                            rows: 1,
                                            isTransparent: true))
              .Add(ObjectMaterial.SoftTissues,
                   new TextureAtlasResource("FX/HitSparks/HitSparksSoftTissue",
                                            columns: 4,
                                            rows: 1,
                                            isTransparent: true))
              .Add(ObjectMaterial.Vegetation,
                   new TextureAtlasResource("FX/HitSparks/HitSparksVegetation",
                                            columns: 4,
                                            rows: 1,
                                            isTransparent: true));

        public static readonly IReadOnlyWeaponHitSparksPreset Laser
            = new WeaponHitSparksPreset()
                .SetDefault(new TextureAtlasResource("FX/HitSparks/HitSparksLaser",
                                                     columns: 4,
                                                     rows: 1,
                                                     isTransparent: true),
                            lightColor: LightColors.WeaponFireMuzzleFlashLaser.WithAlpha(0x99));

        public static readonly IReadOnlyWeaponHitSparksPreset Plasma
            = new WeaponHitSparksPreset()
                .SetDefault(new TextureAtlasResource("FX/HitSparks/HitSparksPlasma",
                                                     columns: 4,
                                                     rows: 1,
                                                     isTransparent: true),
                            lightColor: LightColors.WeaponFireMuzzleFlashPlasma.WithAlpha(0x99));
    }
}