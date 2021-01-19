namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class TileLakeShore : ProtoTile
    {
        private static readonly TextureResource GroundTexture1
            = new("Terrain/LakeShore/TileLakeshoreSand1.jpg",
                  isTransparent: false);

        private static readonly TextureResource GroundTexture2
            = new("Terrain/LakeShore/TileLakeshoreSand2.jpg",
                  isTransparent: false);

        public override byte BlendOrder => 3;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Solid;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Lake shore";

        public override string WorldMapTexturePath
            => "Map/LakeShore.png";

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AmbientSoundProvider = new TileAmbientSoundProvider(
                new AmbientSoundPreset(new SoundResource("Ambient/LakeShore")));

            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture2,
                    blendMaskTexture: BlendMaskTextureSpraySmooth,
                    noiseSelector: null));

            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexture1,
                    blendMaskTexture: BlendMaskTextureSpraySmooth,
                    noiseSelector: new CombinedNoiseSelector(
                        new NoiseSelector(
                            from: 0.4,
                            to: 0.58,
                            noise: new PerlinNoise(seed: 785999248,
                                                   scale: 13,
                                                   octaves: 4,
                                                   persistance: 0.5,
                                                   lacunarity: 1.6)),
                        new NoiseSelector(
                            from: 0.4,
                            to: 0.58,
                            noise: new PerlinNoise(seed: 1643061459,
                                                   scale: 10,
                                                   octaves: 4,
                                                   persistance: 0.4,
                                                   lacunarity: 1.44)))));

            // add pebbles decals
            settings.AddDecal(
                new ProtoTileDecal(ProtoTileDecal.CollectTextures("Terrain/Lakeshore/LakeshorePebbles*"),
                                   size: (2, 2),
                                   drawOrder: DrawOrder.GroundDecalsUnder,
                                   requiresCompleteProtoTileCoverage: true,
                                   noiseSelector:
                                   new CombinedNoiseSelector(
                                       new NoiseSelector(
                                           from: 0.975,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 12831623)),
                                       new NoiseSelector(
                                           from: 0.975,
                                           to: 1,
                                           noise: new WhiteNoise(seed: 346263467)))));
        }
    }
}