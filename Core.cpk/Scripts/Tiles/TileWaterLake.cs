namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Resources;

    public class TileWaterLake : ProtoTileWater
    {
        public override byte BlendOrder => byte.MaxValue;

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

        protected override Color WaterColor => Color.FromArgb(192, 44, 102, 102);

        protected override void PrepareProtoTile(Settings settings)
        {
            base.PrepareProtoTile(settings);
            settings.AmbientSoundProvider = new TileAmbientSoundProvider(
                new AmbientSoundPreset(new SoundResource("Ambient/LakeShore")));
        }
    }
}