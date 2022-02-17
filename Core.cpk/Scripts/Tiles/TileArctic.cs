namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using System;
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class TileArctic : ProtoTile, IProtoTileCold
    {
        private static readonly TextureResource GroundTexture1
            = new("Terrain/Arctic/TileArctic1.jpg",
                  isTransparent: false);

        public override byte BlendOrder => 1;

        // Snow is limiting the movement speed a bit.
        public override double CharacterMoveSpeedMultiplier => 0.9;

        public override TextureAtlasResource CliffAtlas { get; }
            = new("Terrain/Cliffs/TerrainCliffsArctic.png",
                  columns: 6,
                  rows: 4,
                  isTransparent: true);

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Snow;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Arctic";

        public override string WorldMapTexturePath
            => "Map/Arctic.png";

        public static void SharedAddSnowDecals(Settings settings, double density)
        {
            density = MathHelper.Clamp(density, 0, 1);

            // add snow decals
            var snowTexturesUnder = ProtoTileDecal.CollectTextures("Terrain/Arctic/Snow*");
            var snowTexturesMid = ProtoTileDecal.CollectTextures("Terrain/Arctic/SnowMid*");
            var snowTexturesOver = ProtoTileDecal.CollectTextures("Terrain/Arctic/SnowOver*");

            for (ushort x = 0; x <= 1; x++)
            for (ushort y = 0; y <= 1; y++)
            {
                settings.AddDecal(
                    new ProtoTileDecal(snowTexturesUnder,
                                       size: (2, 2),
                                       offset: (x, y),
                                       drawOrder: DrawOrder.GroundDecalsUnder,
                                       noiseSelector: new NoiseSelector(
                                           from: Math.Pow(0.3, density),
                                           to: 1,
                                           noise: new WhiteNoise(seed: 790546823))));
            }

            settings.AddDecal(
                new ProtoTileDecal(snowTexturesMid,
                                   size: (2, 2),
                                   offset: (1, 1),
                                   drawOrder: DrawOrder.GroundDecals,
                                   noiseSelector: new NoiseSelector(
                                       from: Math.Pow(0.45, density),
                                       to: 1,
                                       noise: new WhiteNoise(seed: 62976124))));

            settings.AddDecal(
                new ProtoTileDecal(snowTexturesOver,
                                   size: (2, 2),
                                   offset: (0, 0),
                                   drawOrder: DrawOrder.GroundDecalsOver,
                                   noiseSelector: new NoiseSelector(
                                       from: Math.Pow(0.45, density),
                                       to: 1,
                                       noise: new WhiteNoise(seed: 346212934))));
        }

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AmbientSoundProvider = new TileForestAmbientSoundProvider(
                daySoundPresetPlains: new AmbientSoundPreset(new SoundResource("Ambient/BorealPlains")),
                daySoundPresetForest: new AmbientSoundPreset(new SoundResource("Ambient/BorealForest")),
                nightSoundPresetPlains: new AmbientSoundPreset(new SoundResource("Ambient/BorealPlainsNight")),
                nightSoundPresetForest: new AmbientSoundPreset(new SoundResource("Ambient/BorealForestNight")));

            var groundTexture1 = new ProtoTileGroundTexture(
                texture: GroundTexture1,
                blendMaskTexture: BlendMaskTextureSprayStraightRough,
                noiseSelector: null);

            settings.AddGroundTexture(groundTexture1);

            SharedAddSnowDecals(settings, density: 1.0);
        }
    }
}