namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class TileBarren : ProtoTile
    {
        private static readonly TextureResource GroundTexture1
            = new TextureResource("Terrain/Barren/TileBarren1.jpg",
                                  isTransparent: false);

        private static readonly TextureResource GroundTexture2
            = new TextureResource("Terrain/Barren/TileBarren2.jpg",
                                  isTransparent: false);

        private static readonly TextureResource GroundTextureAtlas3
            = new TextureResource("Terrain/Barren/TileBarren3.jpg",
                                  isTransparent: false);

        public override byte BlendOrder => 7;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Stone;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Barren";

        public override string WorldMapTexturePath
            => "Map/Barren.png";

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AmbientSoundProvider = new TileAmbientSoundProvider(
                new AmbientSoundPreset(new SoundResource("Ambient/Barren")),
                new AmbientSoundPreset(new SoundResource("Ambient/BarrenNight")));

            // ground
            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture2,
                    blendMaskTexture: BlendMaskTextureSprayStraightSmooth,
                    noiseSelector: null));

            // ground with cracks
            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture1,
                    blendMaskTexture: BlendMaskTextureSprayStraightSmooth,
                    noiseSelector: new NoiseSelector(from: 0.4,
                                                     to: 0.7,
                                                     noise: new PerlinNoise(seed: 90230104,
                                                                            scale: 30,
                                                                            octaves: 5,
                                                                            persistance: 0.7,
                                                                            lacunarity: 1.5))));
            // ground with dark "pit" areas
            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTextureAtlas3,
                    blendMaskTexture: BlendMaskTextureSprayStraightSmooth,
                    noiseSelector: new NoiseSelector(from: 0.4,
                                                     to: 0.7,
                                                     noise: new PerlinNoise(seed: 345734573,
                                                                            scale: 25,
                                                                            octaves: 5,
                                                                            persistance: 0.7,
                                                                            lacunarity: 1.5))));

            // add grey stones decals
            var grayStonesTextures = ProtoTileDecal.CollectTextures("Terrain/Barren/Stones*");
            var grayStonesSize = new Vector2Ushort(2, 2);
            var grayStonesNoiseSelector = new CombinedNoiseSelector(
                new NoiseSelector(
                    from: 0.63,
                    to: 0.65,
                    noise: new PerlinNoise(seed: 12313213,
                                           scale: 5,
                                           octaves: 3,
                                           persistance: 0.6,
                                           lacunarity: 1.7)),
                new NoiseSelector(
                    from: 0.63,
                    to: 0.65,
                    noise: new PerlinNoise(seed: 45435435,
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

            // add stick decal
            settings.AddDecal(
                new ProtoTileDecal("Terrain/Barren/Stick*",
                                   size: (1, 1),
                                   drawOrder: DrawOrder.GroundDecalsOver,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   noiseSelector: new NoiseSelector(
                                       from: 0.98,
                                       to: 1,
                                       noise: new WhiteNoise(seed: 742646592))));

            // add bush thorn decal
            settings.AddDecal(
                new ProtoTileDecal("Terrain/Barren/BushThorns*",
                                   size: (1, 1),
                                   drawOrder: DrawOrder.GroundDecalsOver,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   noiseSelector: new NoiseSelector(
                                       from: 0.98,
                                       to: 1,
                                       noise: new WhiteNoise(seed: 634636172))));
        }
    }
}