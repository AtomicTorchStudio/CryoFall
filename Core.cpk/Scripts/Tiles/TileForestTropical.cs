namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class TileForestTropical : ProtoTile, IProtoTileFarmAllowed, IProtoTileWellAllowed
    {
        private static readonly TextureResource GroundTexture1
            = new TextureResource("Terrain/ForestTropical/TileJungle1.jpg",
                                  isTransparent: false);

        private static readonly TextureResource GroundTexture2
            = new TextureResource("Terrain/ForestTropical/TileJungle2.jpg",
                                  isTransparent: false);

        private static readonly TextureResource GroundTextureAtlas3
            = new TextureResource("Terrain/ForestTropical/TileJungle3.jpg",
                                  isTransparent: false);

        public override byte BlendOrder => 8;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Vegetation;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Forest (Tropical)";

        public override string WorldMapTexturePath
            => "Map/ForestTropical.png";

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AmbientSoundProvider = new TileForestAmbientSoundProvider(
                daySoundPresetPlains: new AmbientSoundPreset(new SoundResource("Ambient/TropicalPlains")),
                daySoundPresetForest: new AmbientSoundPreset(new SoundResource("Ambient/TropicalForest")),
                nightSoundPresetPlains: new AmbientSoundPreset(new SoundResource("Ambient/TropicalPlainsNight")),
                nightSoundPresetForest: new AmbientSoundPreset(new SoundResource("Ambient/TropicalForestNight")));

            var groundTextureForest3 = new ProtoTileGroundTexture(
                texture: GroundTextureAtlas3,
                blendMaskTexture: BlendMaskTextureSprayRough,
                noiseSelector: null);

            var groundTextureForest2 = new ProtoTileGroundTexture(
                texture: GroundTexture2,
                blendMaskTexture: BlendMaskTextureSprayRough,
                noiseSelector: new NoiseSelector(from: 0.5,
                                                 to: 0.7,
                                                 noise: new PerlinNoise(seed: 23402375,
                                                                        scale: 4,
                                                                        octaves: 5,
                                                                        persistance: 0.6,
                                                                        lacunarity: 1.8)));

            var groundTextureForest1 = new ProtoTileGroundTexture(
                texture: GroundTexture1,
                blendMaskTexture: BlendMaskTextureSprayRough,
                noiseSelector: new NoiseSelector(from: 0.4,
                                                 to: 0.55,
                                                 noise: new PerlinNoise(seed: 219461235,
                                                                        scale: 5,
                                                                        octaves: 5,
                                                                        persistance: 0.6,
                                                                        lacunarity: 1.6)));
            settings.AddGroundTexture(groundTextureForest3);
            settings.AddGroundTexture(groundTextureForest2);
            settings.AddGroundTexture(groundTextureForest1);

            // add JungleGrass1 decal
            var jungleGrass1Textures = ProtoTileDecal.CollectTextures("Terrain/ForestTropical/JungleGrass1_*");
            var jungleGrass1NoiseSelector = new CombinedNoiseSelector(
                new NoiseSelector(
                    from: 0.5,
                    to: 0.57,
                    noise: new PerlinNoise(seed: 75432132,
                                           scale: 5,
                                           octaves: 5,
                                           persistance: 0.8,
                                           lacunarity: 2)),
                new NoiseSelector(
                    from: 0.73,
                    to: 0.8,
                    noise: new PerlinNoise(seed: 1232165326,
                                           scale: 7,
                                           octaves: 5,
                                           persistance: 0.8,
                                           lacunarity: 2)));

            settings.AddDecal(
                new ProtoTileDecal(jungleGrass1Textures,
                                   size: (2, 2),
                                   drawOrder: DrawOrder.GroundDecalsUnder,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   noiseSelector: jungleGrass1NoiseSelector));

            settings.AddDecal(
                new ProtoTileDecal(jungleGrass1Textures,
                                   size: (2, 2),
                                   offset: (1, 1),
                                   drawOrder: DrawOrder.GroundDecalsUnder,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   noiseSelector: jungleGrass1NoiseSelector));

            // add JungleGrass2 decal
            var jungleGrass2Textures = ProtoTileDecal.CollectTextures("Terrain/ForestTropical/JungleGrass2_*");
            var jungleGrass2NoiseSelector = new CombinedNoiseSelector(
                new NoiseSelector(
                    from: 0.5,
                    to: 0.57,
                    noise: new PerlinNoise(seed: 634231492,
                                           scale: 5,
                                           octaves: 5,
                                           persistance: 0.8,
                                           lacunarity: 2)),
                new NoiseSelector(
                    from: 0.73,
                    to: 0.8,
                    noise: new PerlinNoise(seed: 19831263,
                                           scale: 7,
                                           octaves: 5,
                                           persistance: 0.8,
                                           lacunarity: 2)));

            settings.AddDecal(
                new ProtoTileDecal(jungleGrass2Textures,
                                   size: (2, 2),
                                   drawOrder: DrawOrder.GroundDecalsUnder,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   noiseSelector: jungleGrass2NoiseSelector));

            settings.AddDecal(
                new ProtoTileDecal(jungleGrass2Textures,
                                   size: (2, 2),
                                   offset: (1, 1),
                                   drawOrder: DrawOrder.GroundDecalsUnder,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   noiseSelector: jungleGrass2NoiseSelector));

            // add fern decal
            settings.AddDecal(
                new ProtoTileDecal("Terrain/ForestTropical/Fern*",
                                   size: (1, 1),
                                   drawOrder: DrawOrder.GroundDecalsOver,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   noiseSelector: new CombinedNoiseSelector(
                                       new NoiseSelector(
                                           from: 0.99,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 854712138)),
                                       new NoiseSelector(
                                           from: 0.99,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 907512642)),
                                       new NoiseSelector(
                                           from: 0.99,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 893146123)))));

            // add flower decal
            settings.AddDecal(
                new ProtoTileDecal("Terrain/ForestTropical/JungleFlower*",
                                   size: (1, 1),
                                   drawOrder: DrawOrder.GroundDecalsOver,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   noiseSelector: new CombinedNoiseSelector(
                                       new NoiseSelector(
                                           from: 0.99,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 654352413)),
                                       new NoiseSelector(
                                           from: 0.99,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 219301646)),
                                       new NoiseSelector(
                                           from: 0.99,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 1283912642)),
                                       new NoiseSelector(
                                           from: 0.99,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 1283917341)),
                                       new NoiseSelector(
                                           from: 0.99,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 858366412)))));
        }
    }
}