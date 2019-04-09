namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientShadowHelper
    {
        private static readonly ITextureResource TextureResourceShadow
            // always use max quality sprite, mipmap will do the job
            = new TextureResource("FX/Shadow",
                                  qualityOffset: -100);

        public static IComponentSpriteRenderer AddShadowRenderer(
            IWorldObject worldObject,
            float scaleMultiplier = 1f,
            Vector2D? positionOffset = null)
        {
            if (scaleMultiplier <= 0f)
            {
                return null;
            }

            var spriteRenderer = Api.Client.Rendering.CreateSpriteRenderer(
                worldObject,
                TextureResourceShadow,
                drawOrder: DrawOrder.Shadow,
                positionOffset: positionOffset ?? (0.5, 0.1),
                // draw origin - center
                spritePivotPoint: (0.5, 0.5));

            spriteRenderer.Scale = 0.5 * scaleMultiplier;
            return spriteRenderer;
        }
    }
}