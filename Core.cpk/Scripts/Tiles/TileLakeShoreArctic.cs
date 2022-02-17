namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class TileLakeShoreArctic : ProtoTile, IProtoTileCold
    {
        private static readonly TextureResource GroundTexture1
            = new("Terrain/LakeShoreArctic/TileLakeshoreArcticSand1.jpg",
                  isTransparent: false);

        private static readonly TextureResource GroundTexture2
            = new("Terrain/LakeShoreArctic/TileLakeshoreArcticSand2.jpg",
                  isTransparent: false);

        public override byte BlendOrder => 3;

        // Snow is limiting the movement speed a bit.
        public override double CharacterMoveSpeedMultiplier => 0.9;

        public override TextureAtlasResource CliffAtlas { get; }
            = new("Terrain/Cliffs/TerrainCliffsArctic.png",
                  columns: 6,
                  rows: 4,
                  isTransparent: true);

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Solid;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Lake shore (Arctic)";

        public override string WorldMapTexturePath
            => "Map/LakeShoreArctic.png";

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AmbientSoundProvider = new TileForestAmbientSoundProvider(
                daySoundPresetPlains: new AmbientSoundPreset(new SoundResource("Ambient/BorealPlains")),
                daySoundPresetForest: new AmbientSoundPreset(new SoundResource("Ambient/BorealForest")),
                nightSoundPresetPlains: new AmbientSoundPreset(new SoundResource("Ambient/BorealPlainsNight")),
                nightSoundPresetForest: new AmbientSoundPreset(new SoundResource("Ambient/BorealForestNight")));

            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture2,
                    blendMaskTexture: BlendMaskTextureSpraySmooth,
                    noiseSelector: null));

            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture1,
                    blendMaskTexture: BlendMaskTextureSpraySmooth,
                    noiseSelector: new CombinedNoiseSelector(
                        new NoiseSelector(
                            from: 0.4,
                            to: 0.58,
                            noise: new PerlinNoise(seed: 785999248,
                                                   scale: 13,
                                                   octaves: 4,
                                                   persistance: 0.5,
                                                   lacunarity: 1.6)),
                        new NoiseSelector(
                            from: 0.4,
                            to: 0.58,
                            noise: new PerlinNoise(seed: 1643061459,
                                                   scale: 10,
                                                   octaves: 4,
                                                   persistance: 0.4,
                                                   lacunarity: 1.44)))));

            TileArctic.SharedAddSnowDecals(settings, density: 0.25);
        }
    }
}