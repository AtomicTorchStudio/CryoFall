namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class TileForestTemperate : ProtoTile, IProtoTileFarmAllowed, IProtoTileWellAllowed
    {
        private static readonly TextureResource GroundTexture1
            = new TextureResource("Terrain/ForestTemperate/TileForest1.jpg",
                                  isTransparent: false);

        private static readonly TextureResource GroundTexture2
            = new TextureResource("Terrain/ForestTemperate/TileForest2.jpg",
                                  isTransparent: false);

        private static readonly TextureResource GroundTextureAtlas3
            = new TextureResource("Terrain/ForestTemperate/TileForest3.jpg",
                                  isTransparent: false);

        //public AmbientSoundPreset AdditiveAmbientSoundResourceDayTrees { get; }
        //    = new AmbientSoundPreset(new SoundResource("Ambient/TemperateForest"));

        //public AmbientSoundPreset AdditiveAmbientSoundResourceNightTrees { get; }
        //    = new AmbientSoundPreset(new SoundResource("Ambient/TemperateForestNight"));

        //public  AmbientSoundPreset AmbientSoundResourceDay { get; }
        //    = new AmbientSoundPreset(new SoundResource("Ambient/TemperatePlains"));

        //public AmbientSoundPreset AmbientSoundResourceNight { get; }
        //    = new AmbientSoundPreset(new SoundResource("Ambient/TemperatePlainsNight"));

        public override byte BlendOrder => 8;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Vegetation;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Forest (Temperate)";

        public override string WorldMapTexturePath
            => "Map/ForestTemperate.png";

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AmbientSoundProvider = new TileForestAmbientSoundProvider(
                daySoundPresetPlains: new AmbientSoundPreset(new SoundResource("Ambient/TemperatePlains")),
                daySoundPresetForest: new AmbientSoundPreset(new SoundResource("Ambient/TemperateForest")),
                nightSoundPresetPlains: new AmbientSoundPreset(new SoundResource("Ambient/TemperatePlainsNight")),
                nightSoundPresetForest: new AmbientSoundPreset(new SoundResource("Ambient/TemperateForestNight")));

            // add ground texture for pebbles ground
            var groundTextureForest3 = new ProtoTileGroundTexture(
                texture: GroundTextureAtlas3,
                blendMaskTexture: BlendMaskTextureSprayRough,
                noiseSelector: null);

            settings.AddGroundTexture(groundTextureForest3);

            // add ground texture for dry leaves
            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture2,
                    blendMaskTexture: BlendMaskTextureSprayRough,
                    noiseSelector: new NoiseSelector(from: 0.5,
                                                     to: 1,
                                                     noise: new PerlinNoise(seed: 2010709225,
                                                                            scale: 15,
                                                                            octaves: 5,
                                                                            persistance: 0.6,
                                                                            lacunarity: 1.4))));

            // add ground texture for dry twigs
            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture1,
                    blendMaskTexture: BlendMaskTextureSprayRough,
                    noiseSelector: new NoiseSelector(from: 0.4,
                                                     to: 0.65,
                                                     noise: new PerlinNoise(seed: 1059426846,
                                                                            scale: 23,
                                                                            octaves: 5,
                                                                            persistance: 0.5,
                                                                            lacunarity: 1.4))));
            // add moss decals
            var mossTextures = ProtoTileDecal.CollectTextures("Terrain/ForestTemperate/Moss*");
            var mossDrawOrder = DrawOrder.GroundDecalsUnder;
            var mossSize = new Vector2Ushort(2, 2);
            var mossNoiseSelector = new CombinedNoiseSelector(
                new NoiseSelector(
                    from: 0.8,
                    to: 0.97,
                    noise: new PerlinNoise(seed: 1298138103,
                                           scale: 10,
                                           octaves: 3,
                                           persistance: 0.7,
                                           lacunarity: 1.5)),
                new NoiseSelector(
                    from: 0.85,
                    to: 1,
                    noise: new PerlinNoise(seed: 38602111,
                                           scale: 11,
                                           octaves: 3,
                                           persistance: 0.7,
                                           lacunarity: 3)));

            // add moss decals with a four different offsets (to avoid empty space between neighbor moss sprites)
            for (ushort x = 0; x <= 1; x++)
            for (ushort y = 0; y <= 1; y++)
            {
                settings.AddDecal(
                    new ProtoTileDecal(mossTextures,
                                       size: mossSize,
                                       offset: (x, y),
                                       drawOrder: mossDrawOrder,
                                       noiseSelector: mossNoiseSelector));
            }

            // some random moss
            settings.AddDecal(
                new ProtoTileDecal(mossTextures,
                                   size: mossSize,
                                   drawOrder: mossDrawOrder,
                                   noiseSelector: new NoiseSelector(
                                       from: 0.85,
                                       to: 1,
                                       noise: new WhiteNoise(seed: 306721460))));

            // add clover decals
            var cloverTextures = ProtoTileDecal.CollectTextures("Terrain/ForestTemperate/GrassClover*");
            var cloverSize = new Vector2Ushort(2, 2);
            var cloverNoiseSelector = new CombinedNoiseSelector(
                new NoiseSelector(
                    from: 0.5,
                    to: 0.65,
                    noise: new PerlinNoise(seed: 503902084,
                                           scale: 5,
                                           octaves: 3,
                                           persistance: 0.6,
                                           lacunarity: 1.7)),
                new NoiseSelector(
                    from: 0.55,
                    to: 0.65,
                    noise: new PerlinNoise(seed: 1884332513,
                                           scale: 8,
                                           octaves: 3,
                                           persistance: 0.6,
                                           lacunarity: 1.7)));

            settings.AddDecal(
                new ProtoTileDecal(cloverTextures,
                                   size: cloverSize,
                                   noiseSelector: cloverNoiseSelector));

            // add the same clover but with a little offset (to make a more dense diagonal placement)
            settings.AddDecal(
                new ProtoTileDecal(cloverTextures,
                                   size: cloverSize,
                                   offset: Vector2Ushort.One,
                                   noiseSelector: cloverNoiseSelector));

            // add bush decal
            settings.AddDecal(
                new ProtoTileDecal("Terrain/ForestTemperate/BushGreen*",
                                   size: Vector2Ushort.One,
                                   drawOrder: DrawOrder.GroundDecalsOver,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   noiseSelector: new NoiseSelector(
                                       from: 0.975,
                                       to: 1,
                                       noise: new WhiteNoise(seed: 1092163963))));
        }
    }
}