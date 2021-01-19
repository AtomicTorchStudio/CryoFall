namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TileWaterSea : ProtoTileWater
    {
        private TileWaterSeaBridge bridgeProtoTile;

        public override byte BlendOrder => byte.MaxValue;

        public override IProtoTileWater BridgeProtoTile
            => this.bridgeProtoTile
                   ??= Api.GetProtoEntity<TileWaterSeaBridge>();

        public override bool CanCollect => true;

        public override bool IsFishingAllowed => true;

        public override string Name => "Water (sea)";

        public override TextureResource UnderwaterGroundTextureAtlas { get; }
            = new("Terrain/Beach/TileSand2.jpg",
                  isTransparent: false);

        public override string WorldMapTexturePath
            => "Map/WaterSea.png";

        protected override ITextureResource TextureWaterWorldPlaceholder { get; }
            = new TextureResource("Terrain/Water/TileWaterSeaPlaceholder",
                                  isTransparent: false);

        protected override Color WaterColor => Color.FromArgb(230, 0, 90, 166);

        protected override void PrepareProtoTile(Settings settings)
        {
            base.PrepareProtoTile(settings);
            settings.AmbientSoundProvider = new TileAmbientSoundProvider(
                new AmbientSoundPreset(new SoundResource("Ambient/SeaBeach")));
        }
    }
}