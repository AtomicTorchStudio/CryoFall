namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class TileMeadows : ProtoTile, IProtoTileFarmAllowed, IProtoTileWellAllowed
    {
        private static readonly TextureResource GroundTexture1
            = new TextureResource("Terrain/Meadows/TileMeadows1.jpg",
                                  isTransparent: false);

        private static readonly TextureResource GroundTexture2
            = new TextureResource("Terrain/Meadows/TileMeadows2.jpg",
                                  isTransparent: false);

        public override byte BlendOrder => 20;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Vegetation;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Meadows";

        public override string WorldMapTexturePath
            => "Map/Meadows.png";

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AmbientSoundProvider = new TileAmbientSoundProvider(
                daySoundPreset: new AmbientSoundPreset(new SoundResource("Ambient/Meadows")),
                nightSoundPreset: new AmbientSoundPreset(new SoundResource("Ambient/MeadowsNight")));

            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture1,
                    blendMaskTexture: BlendMaskTextureSprayRough,
                    noiseSelector: null));

            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture2,
                    blendMaskTexture: BlendMaskTextureSprayRough,
                    noiseSelector: new NoiseSelector(from: 0.3,
                                                     to: 0.65,
                                                     noise: new PerlinNoise(seed: 1625285324,
                                                                            scale: 14,
                                                                            octaves: 5,
                                                                            persistance: 0.6,
                                                                            lacunarity: 1.45))));

            // add plants decals
            settings.AddDecalDoubleWithOffset(ProtoTileDecal.CollectTextures("Terrain/Meadows/MeadowPlants*"),
                                              size: (2, 2),
                                              drawOrder: DrawOrder.GroundDecalsUnder,
                                              noiseSelector: new CombinedNoiseSelector(
                                                  // pattern like ways
                                                  new NoiseSelector(
                                                      from: 0.5,
                                                      to: 0.65,
                                                      noise: new PerlinNoise(seed: 123729123,
                                                                             scale: 5,
                                                                             octaves: 3,
                                                                             persistance: 0.6,
                                                                             lacunarity: 1.7)),
                                                  // pattern like ways
                                                  new NoiseSelector(
                                                      from: 0.55,
                                                      to: 0.65,
                                                      noise: new PerlinNoise(seed: 742572457,
                                                                             scale: 8,
                                                                             octaves: 3,
                                                                             persistance: 0.6,
                                                                             lacunarity: 1.7)),
                                                  // random pattern
                                                  new NoiseSelector(
                                                      from: 0.9,
                                                      to: 1,
                                                      noise: new WhiteNoise(seed: 1238127464))));

            // add chamomile decals
            settings.AddDecalDoubleWithOffset(ProtoTileDecal.CollectTextures("Terrain/Meadows/Chamomile*"),
                                              size: (2, 2),
                                              noiseSelector: new NoiseSelector(
                                                  from: 0.85,
                                                  to: 1,
                                                  noise: new PerlinNoise(seed: 829895415,
                                                                         scale: 6,
                                                                         octaves: 4,
                                                                         persistance: 0.3,
                                                                         lacunarity: 1.2)));

            // add flowers decals
            settings.AddDecalDoubleWithOffset(ProtoTileDecal.CollectTextures("Terrain/Meadows/YellowFlowers*"),
                                              size: (2, 2),
                                              noiseSelector: new CombinedNoiseSelector(
                                                  new NoiseSelector(
                                                      from: 0.63,
                                                      to: 0.65,
                                                      noise: new PerlinNoise(seed: 457218345,
                                                                             scale: 5,
                                                                             octaves: 3,
                                                                             persistance: 0.6,
                                                                             lacunarity: 1.7)),
                                                  new NoiseSelector(
                                                      from: 0.63,
                                                      to: 0.65,
                                                      noise: new PerlinNoise(seed: 243492398,
                                                                             scale: 8,
                                                                             octaves: 3,
                                                                             persistance: 0.6,
                                                                             lacunarity: 1.7))));
        }
    }
}