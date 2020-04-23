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
                LightDurationSeconds = 4 / 60.0,
                LightPower = 8.0,
                LightColor = LightColors.WeaponFireMuzzleFlashFirearm,
                LightScreenOffsetRelativeToTexture = (20, 0)
            };

        // Energy weapons
        public static readonly IMuzzleFlashDescriptionReadOnly EnergyLaserWeapon
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasLaser,
                          lightDurationSeconds: 6 / 60.0,
                          lightPower: 10,
                          lightColor: LightColors.WeaponFireMuzzleFlashLaser);

        public static readonly IMuzzleFlashDescriptionReadOnly EnergyPlasmaWeapon
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasPlasma,
                          lightDurationSeconds: 6 / 60.0,
                          lightPower: 12,
                          lightColor: LightColors.WeaponFireMuzzleFlashPlasma);

        // Modern weapons
        public static readonly IMuzzleFlashDescriptionReadOnly ModernHandgun
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasNoSmokeSmall,
                          lightPower: 8);

        public static readonly IMuzzleFlashDescriptionReadOnly ModernRifle
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasNoSmokeLarge,
                          lightPower: 10);

        public static readonly IMuzzleFlashDescriptionReadOnly ModernShotgun
            = Default.Clone()
                     .Set(
                         textureAtlas: MuzzleFlashAtlases.AtlasNoSmokeWide,
                         lightPower: 12,
                         lightDurationSeconds: 5 / 60.0);

        public static readonly IMuzzleFlashDescriptionReadOnly ModernSubmachinegun
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasNoSmokeLarge,
                          lightPower: 8);

        public static readonly IMuzzleFlashDescriptionReadOnly None
            = new MuzzleFlashDescription();

        // Primitive weapons
        public static readonly IMuzzleFlashDescriptionReadOnly PrimitivePistol
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasSmokeSmall1,
                          lightPower: 8);

        public static readonly IMuzzleFlashDescriptionReadOnly PrimitiveRifle
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasSmokeLarge,
                          lightPower: 10);

        // Simple weapons
        public static readonly IMuzzleFlashDescriptionReadOnly SimplePistol
            = Default.Clone()
                     .Set(textureAtlas: MuzzleFlashAtlases.AtlasSmokeSmall2,
                          lightPower: 8);
    }
}