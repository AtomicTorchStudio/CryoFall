namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class TileRuins : ProtoTile
    {
        private static readonly TextureResource GroundTexture1
            = new("Terrain/Ruins/TileRuins1.jpg",
                  isTransparent: false);

        public override byte BlendOrder => 30;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Solid;

        public override bool IsRestrictingConstruction => true;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Ruins";

        public override string WorldMapTexturePath
            => "Map/Ruins.png";

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AmbientSoundProvider = new TileAmbientSoundProvider(
                new AmbientSoundPreset(new SoundResource("Ambient/Ruins"),
                                       suppressionCoef: 1,
                                       isSupressingMusic: true));

            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture1,
                    blendMaskTexture: BlendMaskTextureSprayStraightSmooth,
                    noiseSelector: null));

            // add bricks decals
            var bricksTextures = ProtoTileDecal.CollectTextures("Terrain/Ruins/Bricks*");
            var bricksSize = new Vector2Ushort(2, 2);
            var bricksNoiseSelector = new CombinedNoiseSelector(
                new NoiseSelector(
                    from: 0.6,
                    to: 0.65,
                    noise: new PerlinNoise(seed: 324921396,
                                           scale: 5,
                                           octaves: 3,
                                           persistance: 0.6,
                                           lacunarity: 1.7)),
                new NoiseSelector(
                    from: 0.6,
                    to: 0.65,
                    noise: new PerlinNoise(seed: 324534534,
                                           scale: 8,
                                           octaves: 3,
                                           persistance: 0.6,
                                           lacunarity: 1.7)));

            settings.AddDecalDoubleWithOffset(
                bricksTextures,
                size: bricksSize,
                noiseSelector: bricksNoiseSelector);

            // add various single tile garbage decals
            settings.AddDecal(
                new ProtoTileDecal(ProtoTileDecal.CollectTextures("Terrain/Ruins/Bottle*"),
                                   size: (1, 1),
                                   drawOrder: DrawOrder.GroundDecalsOver,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   noiseSelector: new CombinedNoiseSelector(
                                       new NoiseSelector(
                                           from: 0.98,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 69435135)))));

            settings.AddDecal(
                new ProtoTileDecal(ProtoTileDecal.CollectTextures(
                                       "Terrain/Ruins/Bag*",
                                       "Terrain/Ruins/Cardboard*",
                                       "Terrain/Ruins/Tube*",
                                       "Terrain/Ruins/Tubes*"),
                                   // actually the size is 1x1 but we don't want them to spawn nearby
                                   size: (1, 1),
                                   drawOrder: DrawOrder.GroundDecalsOver,
                                   hidingSetting: DecalHidingSetting.AnyObject,
                                   noiseSelector: new CombinedNoiseSelector(
                                       new NoiseSelector(
                                           from: 0.95,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 73453423)),
                                       new NoiseSelector(
                                           from: 0.95,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 12983622)))));
        }
    }
}