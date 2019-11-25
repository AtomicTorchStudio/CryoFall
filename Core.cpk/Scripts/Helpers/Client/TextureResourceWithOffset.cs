namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Primitives;

    public readonly struct TextureResourceWithOffset
    {
        public readonly Vector2D? Offset;

        public readonly Vector2D? PivotPoint;

        public readonly ITextureResource TextureResource;

        public TextureResourceWithOffset(
            ITextureResource textureResource,
            Vector2D? offset = null,
            Vector2D? pivotPoint = null)
        {
            this.TextureResource = textureResource;
            this.Offset = offset;
            this.PivotPoint = pivotPoint;
        }

        public static implicit operator TextureResourceWithOffset(TextureResource test)
        {
            return new TextureResourceWithOffset(test);
        }
    }
}