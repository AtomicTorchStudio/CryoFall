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
                  texturePivotY: 0.2706,
                  texture: new TextureAtlasResource("FX/HitSparks/HitSparksStone",
                                                    columns: 4,
                                                    rows: 1,
                                                    isTransparent: true))
              .Add(ObjectMaterial.Wood,
                   texturePivotY: 0.2313,
                   texture: new TextureAtlasResource("FX/HitSparks/HitSparksWood",
                                                     columns: 4,
                                                     rows: 1,
                                                     isTransparent: true))
              .Add(ObjectMaterial.Metal,
                   texturePivotY: 0.2187,
                   texture: new TextureAtlasResource("FX/HitSparks/HitSparksMetal",
                                                     columns: 4,
                                                     rows: 1,
                                                     isTransparent: true),
                   lightColor: Color.FromArgb(0x99, 0xFF, 0xEE, 0XAA))
              .Add(ObjectMaterial.HardTissues,
                   texturePivotY: 0.2627,
                   texture: new TextureAtlasResource("FX/HitSparks/HitSparksHardTissue",
                                                     columns: 4,
                                                     rows: 1,
                                                     isTransparent: true))
              .Add(ObjectMaterial.SoftTissues,
                   texturePivotY: 0.2627,
                   texture: new TextureAtlasResource("FX/HitSparks/HitSparksSoftTissue",
                                                     columns: 4,
                                                     rows: 1,
                                                     isTransparent: true))
              .Add(ObjectMaterial.Vegetation,
                   texturePivotY: 0.2627,
                   texture: new TextureAtlasResource("FX/HitSparks/HitSparksVegetation",
                                                     columns: 4,
                                                     rows: 1,
                                                     isTransparent: true));

        public static readonly IReadOnlyWeaponHitSparksPreset LaserBlue
            = new WeaponHitSparksPreset()
                .SetDefault(texturePivotY: 0.2431,
                            texture: new TextureAtlasResource("FX/HitSparks/HitSparksLaserBlue",
                                                              columns: 4,
                                                              rows: 1,
                                                              isTransparent: true),
                            lightColor: LightColors.WeaponFireMuzzleFlashLaserBlue.WithAlpha(0x99),
                            useScreenBlending: true,
                            allowRandomizedHitPointOffset: false);

        public static readonly IReadOnlyWeaponHitSparksPreset LaserMining
            = new WeaponHitSparksPreset()
                .SetDefault(texturePivotY: 0.2246,
                            texture: new TextureAtlasResource("FX/HitSparks/HitSparksLaserMining",
                                                              columns: 4,
                                                              rows: 1,
                                                              isTransparent: true),
                            lightColor: LightColors.LaserMining.WithAlpha(0x99),
                            useScreenBlending: true,
                            allowRandomizedHitPointOffset: false);

        public static readonly IReadOnlyWeaponHitSparksPreset LaserRed
            = new WeaponHitSparksPreset()
                .SetDefault(texturePivotY: 0.2431,
                            texture: new TextureAtlasResource("FX/HitSparks/HitSparksLaserRed",
                                                              columns: 4,
                                                              rows: 1,
                                                              isTransparent: true),
                            lightColor: LightColors.WeaponFireMuzzleFlashLaserRed.WithAlpha(0x99),
                            useScreenBlending: true,
                            allowRandomizedHitPointOffset: false);

        public static readonly IReadOnlyWeaponHitSparksPreset NoWeapon
            = new WeaponHitSparksPreset()
              .SetDefault(texturePivotY: 0.2627,
                          texture: new TextureAtlasResource("FX/HitSparks/HitSparksHardTissue",
                                                            columns: 4,
                                                            rows: 1,
                                                            isTransparent: true))
              .Add(ObjectMaterial.Wood,
                   texturePivotY: 0.2313,
                   texture: new TextureAtlasResource("FX/HitSparks/HitSparksWood",
                                                     columns: 4,
                                                     rows: 1,
                                                     isTransparent: true))
              .Add(ObjectMaterial.HardTissues,
                   texturePivotY: 0.2627,
                   texture: new TextureAtlasResource("FX/HitSparks/HitSparksHardTissue",
                                                     columns: 4,
                                                     rows: 1,
                                                     isTransparent: true))
              .Add(ObjectMaterial.SoftTissues,
                   texturePivotY: 0.2627,
                   texture: new TextureAtlasResource("FX/HitSparks/HitSparksSoftTissue",
                                                     columns: 4,
                                                     rows: 1,
                                                     isTransparent: true))
              .Add(ObjectMaterial.Vegetation,
                   texturePivotY: 0.2627,
                   texture: new TextureAtlasResource("FX/HitSparks/HitSparksVegetation",
                                                     columns: 4,
                                                     rows: 1,
                                                     isTransparent: true));

        public static readonly IReadOnlyWeaponHitSparksPreset Plasma
            = new WeaponHitSparksPreset()
                .SetDefault(texturePivotY: 0.2627,
                            texture: new TextureAtlasResource("FX/HitSparks/HitSparksPlasma",
                                                              columns: 4,
                                                              rows: 1,
                                                              isTransparent: true),
                            lightColor: LightColors.WeaponFireMuzzleFlashPlasma.WithAlpha(0x99),
                            useScreenBlending: true);
    }
}