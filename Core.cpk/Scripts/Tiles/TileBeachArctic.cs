namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class TileBeachArctic : ProtoTile, IProtoTileCold
    {
        private static readonly TextureResource GroundTexture1
            = new("Terrain/BeachArctic/TileBeachArctic1.jpg",
                  isTransparent: false);

        private static readonly TextureResource GroundTexture2
            = new("Terrain/BeachArctic/TileBeachArctic2.jpg",
                  isTransparent: false);

        public override byte BlendOrder => byte.MaxValue;

        // Snow is limiting the movement speed a bit.
        public override double CharacterMoveSpeedMultiplier => 0.9;

        public override TextureAtlasResource CliffAtlas { get; }
            = new("Terrain/Cliffs/TerrainCliffsArctic.png",
                  columns: 6,
                  rows: 4,
                  isTransparent: true);

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Solid;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Beach (Arctic)";

        public override string WorldMapTexturePath
            => "Map/Beach.png";

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AmbientSoundProvider = new TileAmbientSoundProvider(
                new AmbientSoundPreset(new SoundResource("Ambient/SeaBeach")));

            // add ground texture for background (sand without dunes)
            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture2,
                    blendMaskTexture: BlendMaskTextureSpraySmooth,
                    noiseSelector: null));

            // add ground texture for overlay (sand with some dunes)
            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture1,
                    blendMaskTexture: BlendMaskTextureSpraySmooth,
                    noiseSelector: new NoiseSelector(from: 0.4,
                                                     to: 0.65,
                                                     noise: new PerlinNoise(seed: 420557513,
                                                                            scale: 10,
                                                                            octaves: 1,
                                                                            persistance: 0,
                                                                            lacunarity: 1))));

            TileArctic.SharedAddSnowDecals(settings, density: 0.25);
        }
    }
}