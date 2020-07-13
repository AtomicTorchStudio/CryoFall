namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class TileForestBoreal : ProtoTile, IProtoTileFarmAllowed, IProtoTileWellAllowed
    {
        private static readonly TextureResource GroundTexture1
            = new TextureResource("Terrain/ForestBoreal/TileBoreal1.jpg",
                                  isTransparent: false);

        private static readonly TextureResource GroundTexture2
            = new TextureResource("Terrain/ForestBoreal/TileBoreal2.jpg",
                                  isTransparent: false);

        private static readonly TextureResource GroundTextureAtlas3
            = new TextureResource("Terrain/ForestBoreal/TileBoreal3.jpg",
                                  isTransparent: false);

        public override byte BlendOrder => 8;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Vegetation;

        public bool IsStaleWellWater => false;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Forest (Boreal)";

        public override string WorldMapTexturePath
            => "Map/ForestBoreal.png";

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AmbientSoundProvider = new TileForestAmbientSoundProvider(
                daySoundPresetPlains: new AmbientSoundPreset(new SoundResource("Ambient/BorealPlains")),
                daySoundPresetForest: new AmbientSoundPreset(new SoundResource("Ambient/BorealForest")),
                nightSoundPresetPlains: new AmbientSoundPreset(new SoundResource("Ambient/BorealPlainsNight")),
                nightSoundPresetForest: new AmbientSoundPreset(new SoundResource("Ambient/BorealForestNight")));

            var groundTextureForest3 = new ProtoTileGroundTexture(
                texture: GroundTextureAtlas3,
                blendMaskTexture: BlendMaskTextureSprayStraightRough,
                noiseSelector: null);

            var groundTextureForest2 = new ProtoTileGroundTexture(
                texture: GroundTexture2,
                blendMaskTexture: BlendMaskTextureSprayStraightRough,
                noiseSelector: new NoiseSelector(from: 0.5,
                                                 to: 1,
                                                 noise: new PerlinNoise(seed: 392721487,
                                                                        scale: 15,
                                                                        octaves: 5,
                                                                        persistance: 0.6,
                                                                        lacunarity: 1.4)));

            var groundTextureForest1 = new ProtoTileGroundTexture(
                texture: GroundTexture1,
                blendMaskTexture: BlendMaskTextureSprayStraightRough,
                noiseSelector: new NoiseSelector(from: 0.4,
                                                 to: 0.65,
                                                 noise: new PerlinNoise(seed: 498651212,
                                                                        scale: 23,
                                                                        octaves: 5,
                                                                        persistance: 0.5,
                                                                        lacunarity: 1.4)));
            settings.AddGroundTexture(groundTextureForest3);
            settings.AddGroundTexture(groundTextureForest2);
            settings.AddGroundTexture(groundTextureForest1);

            // add smallBushes decals
            var smallBushesTextures = ProtoTileDecal.CollectTextures("Terrain/ForestBoreal/SmallBushes*");
            var smallBushesDrawOrder = DrawOrder.GroundDecalsUnder;
            var smallBushesSize = new Vector2Ushort(2, 2);
            var smallBushesNoiseSelector = new CombinedNoiseSelector(
                new NoiseSelector(
                    from: 0.8,
                    to: 0.97,
                    noise: new PerlinNoise(seed: 209123674,
                                           scale: 10,
                                           octaves: 3,
                                           persistance: 0.7,
                                           lacunarity: 1.5)),
                new NoiseSelector(
                    from: 0.85,
                    to: 1,
                    noise: new PerlinNoise(seed: 129308572,
                                           scale: 11,
                                           octaves: 3,
                                           persistance: 0.7,
                                           lacunarity: 3)));

            // add smallBushes decals with a four different offsets (to avoid empty space between neighbor smallBushes sprites)
            for (ushort x = 0; x <= 1; x++)
            for (ushort y = 0; y <= 1; y++)
            {
                settings.AddDecal(
                    new ProtoTileDecal(smallBushesTextures,
                                       size: smallBushesSize,
                                       offset: (x, y),
                                       drawOrder: smallBushesDrawOrder,
                                       canFlipHorizontally: false,
                                       noiseSelector: smallBushesNoiseSelector));
            }

            // some random small bushes
            settings.AddDecal(
                new ProtoTileDecal(smallBushesTextures,
                                   size: smallBushesSize,
                                   drawOrder: smallBushesDrawOrder,
                                   canFlipHorizontally: false,
                                   noiseSelector: new NoiseSelector(
                                       from: 0.85,
                                       to: 1,
                                       noise: new WhiteNoise(seed: 643613342))));

            // add WhiteFlowers decals
            var whiteFlowersTextures = ProtoTileDecal.CollectTextures("Terrain/ForestBoreal/WhiteFlowers*");
            var whiteFlowersSize = new Vector2Ushort(2, 2);
            var whiteFlowersNoiseSelector = new NoiseSelector(
                from: 0.6,
                to: 0.65,
                noise: new PerlinNoise(seed: 25343634,
                                       scale: 8,
                                       octaves: 3,
                                       persistance: 0.6,
                                       lacunarity: 1.7));

            settings.AddDecal(
                new ProtoTileDecal(whiteFlowersTextures,
                                   size: whiteFlowersSize,
                                   noiseSelector: whiteFlowersNoiseSelector,
                                   requiredGroundTextures: new[] { groundTextureForest1, groundTextureForest3 }));

            // add the same whiteFlowers but with a little offset (to make a more dense diagonal placement)
            settings.AddDecal(
                new ProtoTileDecal(whiteFlowersTextures,
                                   size: whiteFlowersSize,
                                   offset: (1, 1),
                                   noiseSelector: whiteFlowersNoiseSelector,
                                   requiredGroundTextures: new[] { groundTextureForest1, groundTextureForest3 }));

            // add VioletFlowers decals
            var violetFlowersTextures = ProtoTileDecal.CollectTextures("Terrain/ForestBoreal/VioletFlowers*");
            var violetFlowersSize = new Vector2Ushort(2, 2);
            var violetFlowersNoiseSelector = new NoiseSelector(
                from: 0.55,
                to: 0.65,
                noise: new PerlinNoise(seed: 245751427,
                                       scale: 5,
                                       octaves: 3,
                                       persistance: 0.6,
                                       lacunarity: 1.7));

            settings.AddDecal(
                new ProtoTileDecal(violetFlowersTextures,
                                   size: violetFlowersSize,
                                   noiseSelector: violetFlowersNoiseSelector));

            // add the same violetFlowers but with a little offset (to make a more dense diagonal placement)
            settings.AddDecal(
                new ProtoTileDecal(violetFlowersTextures,
                                   size: violetFlowersSize,
                                   offset: (1, 1),
                                   noiseSelector: violetFlowersNoiseSelector));

            // add clover bush decal
            settings.AddDecal(
                new ProtoTileDecal("Terrain/ForestBoreal/CloverBush01*",
                                   size: (1, 1),
                                   drawOrder: DrawOrder.GroundDecalsOver,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   noiseSelector: new NoiseSelector(
                                       from: 0.985,
                                       to: 1,
                                       noise: new WhiteNoise(seed: 982356342))));

            // add fungi decal
            settings.AddDecal(
                new ProtoTileDecal("Terrain/ForestBoreal/Fungi01*",
                                   size: (1, 1),
                                   drawOrder: DrawOrder.GroundDecalsOver,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   noiseSelector: new NoiseSelector(
                                       from: 0.985,
                                       to: 1,
                                       noise: new WhiteNoise(seed: 474631892))));
        }
    }
}