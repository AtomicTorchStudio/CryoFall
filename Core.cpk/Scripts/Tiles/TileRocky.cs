namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class TileRocky : ProtoTile
    {
        private static readonly TextureResource GroundTexture1
            = new("Terrain/Rocky/TileRocky1.jpg",
                  isTransparent: false);

        private static readonly TextureResource GroundTexture2
            = new("Terrain/Rocky/TileRocky2.jpg",
                  isTransparent: false);

        public override byte BlendOrder => 9;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Stone;

        public override bool IsRestrictingConstruction => true;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Rocky mountains";

        public override string WorldMapTexturePath
            => "Map/Rocky.png";

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AmbientSoundProvider = new TileAmbientSoundProvider(
                new AmbientSoundPreset(new SoundResource("Ambient/Rocky"),
                                       suppressionCoef: 1));

            // rock plates
            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture2,
                    blendMaskTexture: BlendMaskTextureGeneric2Smooth,
                    noiseSelector: null));

            // small stones
            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture1,
                    blendMaskTexture: BlendMaskTextureSprayRough,
                    noiseSelector: new CombinedNoiseSelector(
                        new NoiseSelector(from: 0.7,
                                          to: 1,
                                          noise: new PerlinNoise(seed: 345435345,
                                                                 scale: 23,
                                                                 octaves: 5,
                                                                 persistance: 0.5,
                                                                 lacunarity: 1.7)),
                        new NoiseSelector(from: 0.4,
                                          to: 0.6,
                                          noise: new PerlinNoise(seed: 1232132131,
                                                                 scale: 64,
                                                                 octaves: 5,
                                                                 persistance: 0.5,
                                                                 lacunarity: 1.8)))));

            // add grey stones decals
            var grayStonesTextures = ProtoTileDecal.CollectTextures("Terrain/Rocky/GreyStones*");
            var grayStonesSize = new Vector2Ushort(2, 2);
            var grayStonesNoiseSelector = new CombinedNoiseSelector(
                new NoiseSelector(
                    from: 0.63,
                    to: 0.65,
                    noise: new PerlinNoise(seed: 764567465,
                                           scale: 5,
                                           octaves: 3,
                                           persistance: 0.6,
                                           lacunarity: 1.7)),
                new NoiseSelector(
                    from: 0.63,
                    to: 0.65,
                    noise: new PerlinNoise(seed: 453443534,
                                           scale: 8,
                                           octaves: 3,
                                           persistance: 0.6,
                                           lacunarity: 1.7)));

            for (ushort x = 0; x <= 1; x++)
            for (ushort y = 0; y <= 1; y++)
            {
                settings.AddDecal(
                    new ProtoTileDecal(grayStonesTextures,
                                       size: grayStonesSize,
                                       offset: (x, y),
                                       noiseSelector: grayStonesNoiseSelector));
            }

            // add red grass decals
            var redGrassTextures = ProtoTileDecal.CollectTextures("Terrain/Rocky/RedGrass*");
            var redGrassSize = new Vector2Ushort(2, 2);
            var redGrassHidingSetting = DecalHidingSetting.AnyObject;
            var redGrassDrawOrder = DrawOrder.GroundDecalsOver;
            var redGrassNoiseSelector = new CombinedNoiseSelector(
                new NoiseSelector(from: 0.92,
                                  to: 1,
                                  noise: new PerlinNoise(seed: 534543534,
                                                         scale: 5,
                                                         octaves: 3,
                                                         persistance: 0.5,
                                                         lacunarity: 1.7)),
                new NoiseSelector(from: 0.92,
                                  to: 1,
                                  noise: new PerlinNoise(seed: 234234234,
                                                         scale: 5,
                                                         octaves: 3,
                                                         persistance: 0.5,
                                                         lacunarity: 1.7)));

            settings.AddDecal(
                new ProtoTileDecal(redGrassTextures,
                                   size: redGrassSize,
                                   noiseSelector: redGrassNoiseSelector,
                                   hidingSetting: redGrassHidingSetting,
                                   drawOrder: redGrassDrawOrder));

            // add the same redGrass but with a little offset (to make a more dense diagonal placement)
            settings.AddDecal(
                new ProtoTileDecal(redGrassTextures,
                                   size: redGrassSize,
                                   offset: (1, 1),
                                   noiseSelector: redGrassNoiseSelector,
                                   hidingSetting: redGrassHidingSetting,
                                   drawOrder: redGrassDrawOrder));
        }
    }
}