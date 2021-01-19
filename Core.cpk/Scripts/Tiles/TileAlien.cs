namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class TileAlien : ProtoTile
    {
        private static readonly TextureResource GroundTexture1
            = new("Terrain/Alien/TileAlien1.jpg",
                  isTransparent: false);

        public override byte BlendOrder => 9;

        public override TextureAtlasResource CliffAtlas { get; }
            = new("Terrain/Cliffs/TerrainCliffsVolcanic.png",
                  columns: 6,
                  rows: 4,
                  isTransparent: true);

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Stone;

        public override bool IsRestrictingConstruction => true;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Alien";

        public override string WorldMapTexturePath
            => "Map/Alien.png";

        protected override void PrepareProtoTile(Settings settings)
        {
            // Currently the alien biome is used only for the small teleport areas.
            // It reuses the ruins ambient sound as it's pretty scary - exactly as needed for this alien biome.
            settings.AmbientSoundProvider = new TileAmbientSoundProvider(
                new AmbientSoundPreset(new SoundResource("Ambient/Ruins"),
                                       suppressionCoef: 1,
                                       isSupressingMusic: true));

            // terrain
            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture1,
                    blendMaskTexture: BlendMaskTextureGeneric2Smooth,
                    noiseSelector: null));

            // add craters
            settings.AddDecalDoubleWithOffset(
                ProtoTileDecal.CollectTextures("Terrain/Alien/AlienCraters*"),
                size: (2, 2),
                drawOrder: DrawOrder.GroundDecals,
                requiresCompleteProtoTileCoverage: true,
                canFlipHorizontally: true,
                noiseSelector:
                new CombinedNoiseSelector(
                    new NoiseSelector(
                        from: 0.9,
                        to: 1,
                        noise: new WhiteNoise(seed: 962530623)),
                    new NoiseSelector(
                        from: 0.9,
                        to: 1,
                        noise: new WhiteNoise(seed: 743097649))));

            // add small formations
            settings.AddDecal(
                new ProtoTileDecal("Terrain/Alien/AlienFormationSmall*",
                                   size: (1, 1),
                                   drawOrder: DrawOrder.GroundDecalsUnder,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   canFlipHorizontally: true,
                                   noiseSelector: new NoiseSelector(
                                       from: 0.85,
                                       to: 1,
                                       noise: new WhiteNoise(seed: 790546823))));

            // add large formations
            settings.AddDecal(
                new ProtoTileDecal("Terrain/Alien/AlienFormationLarge*",
                                   size: (2, 2),
                                   drawOrder: DrawOrder.GroundDecalsOver,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   canFlipHorizontally: true,
                                   noiseSelector: new NoiseSelector(
                                       from: 0.92,
                                       to: 1,
                                       noise: new WhiteNoise(seed: 790546823))));
        }
    }
}