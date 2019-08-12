namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class TileSwamp : ProtoTile
    {
        private static readonly TextureResource GroundTextureOverlay
            = new TextureResource("Terrain/Swamp/TileSwamp1.jpg",
                                  isTransparent: false);

        private static readonly TextureResource GroundTexturePrimary
            = new TextureResource("Terrain/Swamp/TileSwamp2.jpg",
                                  isTransparent: false);

        public override byte BlendOrder => 9;

        // Swamp is limiting the movement speed a bit.
        public override double CharacterMoveSpeedMultiplier => 0.9;

        public override GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Mud;

        public override bool IsRestrictingConstruction => false;

        public override TileKind Kind => TileKind.Solid;

        public override string Name => "Swamp";

        public override string WorldMapTexturePath
            => "Map/Swamp.png";

        protected override void PrepareProtoTile(Settings settings)
        {
            settings.AmbientSoundProvider = new TileAmbientSoundProvider(
                new AmbientSoundPreset(new SoundResource("Ambient/Swamp")),
                new AmbientSoundPreset(new SoundResource("Ambient/SwampNight")));

            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTexturePrimary,
                    blendMaskTexture: BlendMaskTextureGeneric2Smooth,
                    noiseSelector: null));

            settings.AddGroundTexture(
                new ProtoTileGroundTexture(
                    texture: GroundTextureOverlay,
                    blendMaskTexture: BlendMaskTextureGeneric2Smooth,
                    noiseSelector:
                    new NoiseSelector(
                        from: 0.75,
                        to: 1,
                        new WhiteNoise(21893823))));

            // add moss decals
            settings.AddDecalDoubleWithOffset(
                ProtoTileDecal.CollectTextures("Terrain/Swamp/SwampMoss*"),
                size: new Vector2Ushort(2, 2),
                drawOrder: DrawOrder.GroundDecals,
                canFlipHorizontally: false,
                noiseSelector: new NoiseSelector(
                    from: 0.8,
                    to: 1,
                    noise: new WhiteNoise(seed: 219832163)));

            // some random small grass
            settings.AddDecalDoubleWithOffset(
                ProtoTileDecal.CollectTextures("Terrain/Swamp/SwampGrass*"),
                size: (1, 1),
                drawOrder: DrawOrder.GroundDecalsOver,
                noiseSelector: new NoiseSelector(
                    from: 0.77,
                    to: 1,
                    noise: new WhiteNoise(seed: 1459921236)));

            // some random small bushes
            settings.AddDecalDoubleWithOffset(
                ProtoTileDecal.CollectTextures("Terrain/Swamp/SwampBush*"),
                size: (1, 1),
                interval: (2, 2),
                drawOrder: DrawOrder.GroundDecalsOver,
                noiseSelector: new NoiseSelector(
                    from: 0.5,
                    to: 1,
                    noise: new WhiteNoise(seed: 975129352)));

            // some random fungi
            settings.AddDecal(
                new ProtoTileDecal(ProtoTileDecal.CollectTextures("Terrain/Swamp/SwampFungi*"),
                                   size: (1, 1),
                                   offset: (1, 1),
                                   interval: (2, 2),
                                   drawOrder: DrawOrder.GroundDecalsUnder,
                                   noiseSelector: new NoiseSelector(
                                       from: 0.6,
                                       to: 1,
                                       noise: new WhiteNoise(seed: 346223467))));

            // add branch decals
            settings.AddDecalDoubleWithOffset(
                ProtoTileDecal.CollectTextures("Terrain/Swamp/SwampBranch*"),
                size: (1, 1),
                interval: (2, 2),
                drawOrder: DrawOrder.GroundDecals,
                noiseSelector: new NoiseSelector(
                    from: 0.65,
                    to: 1,
                    noise: new WhiteNoise(seed: 754374734)));
        }
    }
}