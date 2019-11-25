namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class TileBeachBoreal : ProtoTile
    {
        private static readonly TextureResource GroundTexture1
            = new TextureResource("Terrain/Beach/TileSand1.jpg",
                                  isTransparent: false);

        private static readonly TextureResource GroundTexture2
            = new TextureResource("Terrain/Beach/TileSand2.jpg",
                                  isTransparent: false);

        public override byte BlendOrder => 2;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Solid;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Beach (Boreal)";

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
            // add shells & pebbles decals
            settings.AddDecal(
                new ProtoTileDecal(ProtoTileDecal.CollectTextures("Terrain/Beach/ShellsNPebbles*"),
                                   size: (2, 2),
                                   drawOrder: DrawOrder.GroundDecalsUnder,
                                   requiresCompleteProtoTileCoverage: true,
                                   noiseSelector: new NoiseSelector(
                                       from: 0.87,
                                       to: 1,
                                       noise: new WhiteNoise(seed: 742646592))));

            // add dry grass decals
            settings.AddDecal(
                new ProtoTileDecal("Terrain/Beach/Grass*",
                                   size: (1, 1),
                                   drawOrder: DrawOrder.GroundDecalsOver,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   requiresCompleteProtoTileCoverage: true,
                                   noiseSelector: new NoiseSelector(
                                       from: 0.94,
                                       to: 1,
                                       noise: new WhiteNoise(seed: 173453622))));

            // note: unlike temperate beach boreal beach doesn't have individual shells
        }
    }
}