namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class TileSaltFlats : ProtoTile
    {
        private static readonly TextureResource GroundTexture1
            = new TextureResource("Terrain/SaltFlats/TileSaltFlats1.jpg",
                                  isTransparent: false);

        public override byte BlendOrder => 9;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Solid;

        public override bool IsRestrictingConstruction => true;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Salt flats";

        public override string WorldMapTexturePath
            => "Map/SaltFlats.png";

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AmbientSoundProvider = new TileAmbientSoundProvider(
                new AmbientSoundPreset(new SoundResource("Ambient/Pit")));

            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture1,
                    blendMaskTexture: BlendMaskTextureGeneric2Smooth,
                    noiseSelector: null));

            // add formations decals
            var formationsTextures = ProtoTileDecal.CollectTextures("Terrain/SaltFlats/SaltFlatsFormations01*");
            var formationsSize = new Vector2Ushort(2, 2);
            var formationsNoiseSelector = new NoiseSelector(
                from: 0.5,
                to: 0.58,
                noise: new PerlinNoise(seed: 470398528,
                                       scale: 5,
                                       octaves: 4,
                                       persistance: 0.9,
                                       lacunarity: 3.9));

            for (ushort x = 0; x <= 1; x++)
            for (ushort y = 0; y <= 1; y++)
            {
                settings.AddDecal(
                    new ProtoTileDecal(formationsTextures,
                                       size: formationsSize,
                                       offset: (x, y),
                                       noiseSelector: formationsNoiseSelector));
            }

            // add pothole decals
            settings.AddDecal(
                new ProtoTileDecal("Terrain/SaltFlats/SaltFlatsPothole*",
                                   size: (2, 2),
                                   drawOrder: DrawOrder.GroundDecals,
                                   noiseSelector: new CombinedNoiseSelector(
                                       new NoiseSelector(
                                           from: 0.97,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 901267462)),
                                       new NoiseSelector(
                                           from: 0.97,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 68091273)))));

            // add gully decals
            settings.AddDecal(
                new ProtoTileDecal(ProtoTileDecal.CollectTextures("Terrain/SaltFlats/SaltFlatsGully*"),
                                   size: (2, 2),
                                   drawOrder: DrawOrder.GroundDecalsUnder,
                                   noiseSelector: new CombinedNoiseSelector(
                                       new NoiseSelector(
                                           from: 0.98,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 981461233)),
                                       new NoiseSelector(
                                           from: 0.98,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 598236742)),
                                       new NoiseSelector(
                                           from: 0.98,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 98347443)),
                                       new NoiseSelector(
                                           from: 0.98,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 1324234897)),
                                       new NoiseSelector(
                                           from: 0.98,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 324643342)))));
        }
    }
}