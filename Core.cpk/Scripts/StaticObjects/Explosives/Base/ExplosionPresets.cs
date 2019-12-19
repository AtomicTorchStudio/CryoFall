namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ExplosionPresets
    {
        public static readonly ExplosionPreset Large
            = ExplosionPreset.CreatePreset(
                protoObjectCharredGround: Api.GetProtoEntity<ObjectCharredGround1>(),
                serverDamageApplyDelay: 0.8 * 0.25,
                soundSetPath: "Explosions/ExplosionLarge",
                spriteAnimationDuration: 0.8,
                spriteSetPath: "FX/Explosions/ExplosionLarge",
                spriteAtlasColumns: 8,
                spriteAtlasRows: 3,
                spriteWorldSize: new Size2F(2, 2),
                blastwaveDelay: 0.1,
                blastwaveAnimationDuration: 0.6,
                blastWaveColor: Color.FromRgb(0xFF, 0xBB, 0x33),
                blastwaveWorldSizeFrom: 0.25 * new Size2F(3, 2),
                blastwaveWorldSizeTo: 1 * new Size2F(3,      2),
                lightDuration: 1,
                lightWorldSize: 35,
                lightColor: Color.FromRgb(0xFF, 0xCC, 0x66),
                screenShakesDuration: 0.2,
                screenShakesWorldDistanceMin: 0.2,
                screenShakesWorldDistanceMax: 0.25);

        public static readonly ExplosionPreset PragmiumResonanceBomb_Center
            = ExplosionPreset.CreatePreset(
                protoObjectCharredGround: Api.GetProtoEntity<ObjectCharredGround1>(),
                serverDamageApplyDelay: 0.8 * 0.25 + 0.5,
                soundSetPath: "Explosions/ExplosionPragmium",
                spriteAnimationDuration: 0.9,
                spriteSetPath: "FX/Explosions/ExplosionLarge3",
                spriteAtlasColumns: 8,
                spriteAtlasRows: 3,
                spriteWorldSize: new Size2F(3, 3),
                spriteDrawOrder: DrawOrder.Light + 1,
                blastwaveDelay: 0.1,
                blastwaveAnimationDuration: 0.75,
                blastWaveColor: Color.FromRgb(0xBB, 0xDD, 0xFF),
                blastwaveWorldSizeFrom: 0.25 * new Size2F(6, 4),
                blastwaveWorldSizeTo: 1 * new Size2F(6,      4),
                lightDuration: 1.4,
                lightWorldSize: 37.5,
                lightColor: Color.FromRgb(0x88, 0xDD, 0xFF),
                screenShakesDuration: 0, //0.5,
                screenShakesWorldDistanceMin: 0.2,
                screenShakesWorldDistanceMax: 0.25,
                spriteColorAdditive: Color.FromRgb(0x00,       0x44, 0x88),
                spriteColorMultiplicative: Color.FromRgb(0xAA, 0xFF, 0xFF),
                spriteBrightness: 1.33);

        public static readonly ExplosionPreset PragmiumResonanceBomb_NodeClientOnly
            = ExplosionPreset.CreatePreset(
                // not used as it's only for the client
                protoObjectCharredGround: Api.GetProtoEntity<ObjectCharredGround1>(),
                // not used as it's only for the client
                serverDamageApplyDelay: 0,
                soundSetPath: "Explosions/ExplosionPragmium", // should be played without sound
                spriteAnimationDuration: 0.75 * 0.9,
                spriteSetPath: "FX/Explosions/ExplosionLarge3",
                spriteAtlasColumns: 8,
                spriteAtlasRows: 3,
                spriteWorldSize: new Size2F(2, 2),
                spriteDrawOrder: DrawOrder.Light,
                blastwaveDelay: 0.1,
                blastwaveAnimationDuration: 0.75 * 0.75,
                blastWaveColor: Color.FromArgb(0x99, 0xBB, 0xDD, 0xFF),
                blastwaveWorldSizeFrom: 0.667 * 0.25 * new Size2F(6, 4),
                blastwaveWorldSizeTo: 0.667 * 1 * new Size2F(6,      4),
                lightDuration: 0,
                lightWorldSize: 0,
                lightColor: Colors.Transparent,
                screenShakesDuration: 0,
                screenShakesWorldDistanceMin: 0,
                screenShakesWorldDistanceMax: 0,
                spriteColorAdditive: Color.FromRgb(0x00, 0x44, 0x88),
                spriteColorMultiplicative: Color.FromArgb(0xBB, 0xAA, 0xFF, 0xFF),
                spriteBrightness: 1.33);

        public static readonly ExplosionPreset SpecialDepositExplosion
            = ExplosionPreset.CreatePreset(
                protoObjectCharredGround: Api.GetProtoEntity<ObjectCharredGround3Deposit>(),
                serverDamageApplyDelay: 0.8 * 0.25,
                soundSetPath: "Explosions/ExplosionLarge",
                spriteAnimationDuration: 1,
                spriteSetPath: "FX/Explosions/ExplosionLarge3",
                spriteAtlasColumns: 8,
                spriteAtlasRows: 3,
                spriteWorldSize: new Size2F(3, 3),
                blastwaveDelay: 0.1,
                blastwaveAnimationDuration: 0.6,
                blastWaveColor: Color.FromRgb(0xFF, 0xBB, 0x33),
                blastwaveWorldSizeFrom: 0.25 * new Size2F(9, 6),
                blastwaveWorldSizeTo: 1 * new Size2F(9,      6),
                lightDuration: 1.35,
                lightWorldSize: 40,
                lightColor: Color.FromRgb(0xFF, 0xCC, 0x66),
                screenShakesDuration: 0.4,
                screenShakesWorldDistanceMin: 0.3,
                screenShakesWorldDistanceMax: 0.35,
                spriteBrightness: 1.33);

        public static readonly ExplosionPreset SpecialPragmiumSourceExplosion
            = ExplosionPreset.CreatePreset(
                protoObjectCharredGround: Api.GetProtoEntity<ObjectCharredGround2Pragmium>(),
                serverDamageApplyDelay: 0.8 * 0.25,
                soundSetPath: "Explosions/ExplosionPragmium",
                spriteAnimationDuration: 1,
                spriteSetPath: "FX/Explosions/ExplosionLarge3",
                spriteAtlasColumns: 8,
                spriteAtlasRows: 3,
                spriteWorldSize: new Size2F(4, 4),
                blastwaveDelay: 0.1,
                blastwaveAnimationDuration: 0.6,
                blastWaveColor: Color.FromRgb(0xBB, 0xDD, 0xFF),
                blastwaveWorldSizeFrom: 0.25 * new Size2F(9, 6),
                blastwaveWorldSizeTo: 1 * new Size2F(9,      6),
                lightDuration: 1.35,
                lightWorldSize: 40,
                lightColor: Color.FromRgb(0x88, 0xDD, 0xFF),
                screenShakesDuration: 0.4,
                screenShakesWorldDistanceMin: 0.3,
                screenShakesWorldDistanceMax: 0.35,
                spriteColorAdditive: Color.FromRgb(0x00,       0x44, 0x88),
                spriteColorMultiplicative: Color.FromRgb(0xAA, 0xFF, 0xFF),
                spriteBrightness: 1.33);

        public static readonly ExplosionPreset VeryLarge
            = ExplosionPreset.CreatePreset(
                protoObjectCharredGround: Api.GetProtoEntity<ObjectCharredGround1>(),
                serverDamageApplyDelay: 0.8 * 0.25,
                soundSetPath: "Explosions/ExplosionLarge",
                spriteAnimationDuration: 0.8,
                spriteSetPath: "FX/Explosions/ExplosionLarge",
                spriteAtlasColumns: 8,
                spriteAtlasRows: 3,
                spriteWorldSize: new Size2F(2.5, 2.5),
                blastwaveDelay: 0.1,
                blastwaveAnimationDuration: 0.6,
                blastWaveColor: Color.FromRgb(0xFF, 0xBB, 0x33),
                blastwaveWorldSizeFrom: 0.25 * new Size2F(3, 2),
                blastwaveWorldSizeTo: 1 * new Size2F(3,      2),
                lightDuration: 1.35,
                lightWorldSize: 35,
                lightColor: Color.FromRgb(0xFF, 0xCC, 0x66),
                screenShakesDuration: 0.3,
                screenShakesWorldDistanceMin: 0.2,
                screenShakesWorldDistanceMax: 0.25);
    }
}