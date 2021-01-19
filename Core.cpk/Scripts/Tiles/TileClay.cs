namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class TileClay : ProtoTile
    {
        private static readonly TextureResource GroundTexture1
            = new("Terrain/Clay/TileClay1.jpg",
                  isTransparent: false);

        public override byte BlendOrder => 9;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Solid;

        public override bool IsRestrictingConstruction => true;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Clay pit";

        public override string WorldMapTexturePath
            => "Map/Clay.png";

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AmbientSoundProvider = new TileAmbientSoundProvider(
                new AmbientSoundPreset(new SoundResource("Ambient/Pit")));

            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture1,
                    blendMaskTexture: BlendMaskTextureGeneric2Smooth,
                    noiseSelector: null));

            // add clay stones decals
            var clayStonesTextures = ProtoTileDecal.CollectTextures("Terrain/Clay/ClayStones*");
            var clayStonesSize = new Vector2Ushort(2, 2);
            var clayStonesNoiseSelector = new NoiseSelector(
                from: 0.5,
                to: 0.55,
                noise: new PerlinNoise(seed: 642571234,
                                       scale: 5,
                                       octaves: 4,
                                       persistance: 0.9,
                                       lacunarity: 3.9));

            for (ushort x = 0; x <= 1; x++)
            for (ushort y = 0; y <= 1; y++)
            {
                settings.AddDecal(
                    new ProtoTileDecal(clayStonesTextures,
                                       size: clayStonesSize,
                                       offset: (x, y),
                                       noiseSelector: clayStonesNoiseSelector));
            }

            // add clay pit decals
            settings.AddDecal(
                new ProtoTileDecal("Terrain/Clay/ClayPit*",
                                   size: (2, 2),
                                   drawOrder: DrawOrder.GroundDecalsUnder,
                                   noiseSelector: new NoiseSelector(
                                       from: 0.95,
                                       to: 1,
                                       noise: new WhiteNoise(seed: 306721460))));
        }
    }
}