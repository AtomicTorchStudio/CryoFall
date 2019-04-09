namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Noise;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ProtoTileGroundTexture
    {
        public readonly TextureAtlasResource BlendMaskTexture;

        public readonly INoiseSelector NoiseSelector;

        public readonly TextureResource Texture;

        public ProtoTileGroundTexture(
            TextureResource texture,
            ITextureResource blendMaskTexture,
            INoiseSelector noiseSelector)
        {
            this.Texture = texture;

            this.BlendMaskTexture = new TextureAtlasResource(
                blendMaskTexture,
                columns: 4,
                rows: 1);

            this.NoiseSelector = noiseSelector;
        }

        public int CalculatedBlendOrder { get; private set; }

        public ProtoTile ProtoTile { get; private set; }

        public void Prepare(ProtoTile protoTile)
        {
            this.ProtoTile = protoTile;
            this.CalculatedBlendOrder = ProtoTileBlendOrdersCalculator.CalculateBlendOrder(protoTile);
        }
    }
}