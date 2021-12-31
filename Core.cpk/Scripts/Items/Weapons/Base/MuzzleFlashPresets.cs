namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;

    public static class MuzzleFlashPresets
    {
        public static readonly IMuzzleFlashDescriptionReadOnly Default
            = new MuzzleFlashDescription
            {
                TextureAnimationDurationSeconds = 6 / 60.0,
                TextureScale = 1.0,
                TextureAtlas = MuzzleFlashAtlases.AtlasNoSmokeLarge,
                TextureOriginX = MuzzleFlashAtlases.AtlasNoSmokeLargeOriginX,
                LightDurationSeconds = 4 / 60.0,
                LightPower = 8.0,
                LightColor = LightColors.WeaponFireMuzzleFlashFirearm,
                LightScreenOffsetRelativeToTexture = (20, 0)
            };

        public static readonly IMuzzleFlashDescriptionReadOnly Artillery
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasArtillery,
                          textureOriginX: MuzzleFlashAtlases.AtlasArtilleryOriginX,
                          lightPower: 30,
                          lightDurationSeconds: 12 / 30.0,
                          textureAnimationDurationSeconds: 12 / 30.0);

        public static readonly IMuzzleFlashDescriptionReadOnly EnergyLaserWeaponBlue
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasLaserBlue,
                          textureOriginX: MuzzleFlashAtlases.AtlasLaserBlueOriginX,
                          lightDurationSeconds: 6 / 60.0,
                          lightPower: 10,
                          lightColor: LightColors.WeaponFireMuzzleFlashLaserBlue);

        // Energy weapons
        public static readonly IMuzzleFlashDescriptionReadOnly EnergyLaserWeaponRed
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasLaserRed,
                          textureOriginX: MuzzleFlashAtlases.AtlasLaserRedOriginX,
                          lightDurationSeconds: 6 / 60.0,
                          lightPower: 10,
                          lightColor: LightColors.WeaponFireMuzzleFlashLaserRed);

        public static readonly IMuzzleFlashDescriptionReadOnly EnergyLaserWeaponRedLarge
            = EnergyLaserWeaponRed.Clone()
                                  .Set(textureScale: 1.5,
                                       lightDurationSeconds: 12 / 60.0,
                                       lightPower: 12);

        public static readonly IMuzzleFlashDescriptionReadOnly EnergyPlasmaWeapon
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasPlasma,
                          textureOriginX: MuzzleFlashAtlases.AtlasPlasmaOriginX,
                          lightDurationSeconds: 6 / 60.0,
                          lightPower: 12,
                          lightColor: LightColors.WeaponFireMuzzleFlashPlasma);

        public static readonly IMuzzleFlashDescriptionReadOnly GrenadeLauncher
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasSmokeLarge,
                          textureOriginX: MuzzleFlashAtlases.AtlasSmokeLargeOriginX,
                          textureAnimationDurationSeconds: 12 / 60.0,
                          lightPower: 10);

        // Modern weapons
        public static readonly IMuzzleFlashDescriptionReadOnly ModernHandgun
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasNoSmokeSmall,
                          textureOriginX: MuzzleFlashAtlases.AtlasNoSmokeSmallOriginX,
                          lightPower: 8);

        public static readonly IMuzzleFlashDescriptionReadOnly ModernRifle
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasNoSmokeLarge,
                          textureOriginX: MuzzleFlashAtlases.AtlasNoSmokeLargeOriginX,
                          lightPower: 10);

        public static readonly IMuzzleFlashDescriptionReadOnly ModernShotgun
            = Default.Clone()
                     .Set(
                         textureAtlas: MuzzleFlashAtlases.AtlasNoSmokeWide,
                         textureOriginX: MuzzleFlashAtlases.AtlasNoSmokeWideOriginX,
                         lightPower: 12,
                         lightDurationSeconds: 5 / 60.0);

        public static readonly IMuzzleFlashDescriptionReadOnly ModernSubmachinegun
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasNoSmokeLarge,
                          textureOriginX: MuzzleFlashAtlases.AtlasNoSmokeLargeOriginX,
                          lightPower: 8);

        public static readonly IMuzzleFlashDescriptionReadOnly None
            = new MuzzleFlashDescription();

        // Primitive weapons
        public static readonly IMuzzleFlashDescriptionReadOnly PrimitivePistol
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasSmokeSmall1,
                          textureOriginX: MuzzleFlashAtlases.AtlasSmokeSmall1OriginX,
                          lightPower: 8);

        public static readonly IMuzzleFlashDescriptionReadOnly PrimitiveRifle
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasSmokeLarge,
                          textureOriginX: MuzzleFlashAtlases.AtlasSmokeLargeOriginX,
                          lightPower: 10);

        // Simple weapons
        public static readonly IMuzzleFlashDescriptionReadOnly SimplePistol
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasSmokeSmall2,
                          textureOriginX: MuzzleFlashAtlases.AtlasSmokeSmall2OriginX,
                          lightPower: 8);
    }
}