namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Resources;

    public class TileWaterSea : ProtoTileWater
    {
        public override byte BlendOrder => byte.MaxValue;

        public override string Name => "Water (sea)";

        public override TextureResource UnderwaterGroundTextureAtlas { get; }
            = new TextureResource("Terrain/Beach/TileSand2.jpg",
                                  isTransparent: false);

        public override string WorldMapTexturePath
            => "Map/WaterSea.png";

        protected override ITextureResource TextureWaterWorldPlaceholder { get; }
            = new TextureResource("Terrain/Water/TileWaterSeaPlaceholder",
                                  isTransparent: false);

        protected override Color WaterColor => Color.FromArgb(220, 0, 90, 166);

        protected override void PrepareProtoTile(Settings settings)
        {
            base.PrepareProtoTile(settings);
            settings.AmbientSoundProvider = new TileAmbientSoundProvider(
                new AmbientSoundPreset(new SoundResource("Ambient/SeaBeach")));
        }
    }
}