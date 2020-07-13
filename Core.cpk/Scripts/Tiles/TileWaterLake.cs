namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TileWaterLake : ProtoTileWater
    {
        private TileWaterLakeBridge bridgeProtoTile;

        public override byte BlendOrder => byte.MaxValue;

        public override IProtoTileWater BridgeProtoTile
            => this.bridgeProtoTile
                   ??= Api.GetProtoEntity<TileWaterLakeBridge>();

        public override bool IsFishingAllowed => true;

        public override string Name => "Water (lake)";

        public override TextureResource UnderwaterGroundTextureAtlas { get; }
            = new TextureResource("Terrain/LakeShore/TileLakeshoreSand2.jpg",
                                  isTransparent: false);

        public override string WorldMapTexturePath
            => "Map/WaterLake.png";

        protected override float ShoreMaskSpeed => 0.1f;

        protected override ITextureResource TextureWaterWorldPlaceholder { get; }
            = new TextureResource("Terrain/Water/TileWaterLakePlaceholder",
                                  isTransparent: false);

        protected override float WaterAmplitude => 0.025f;

        protected override Color WaterColor => Color.FromArgb(230, 44, 102, 102);

        protected override void PrepareProtoTile(Settings settings)
        {
            base.PrepareProtoTile(settings);
            settings.AmbientSoundProvider = new TileAmbientSoundProvider(
                new AmbientSoundPreset(new SoundResource("Ambient/LakeShore")));
        }
    }
}