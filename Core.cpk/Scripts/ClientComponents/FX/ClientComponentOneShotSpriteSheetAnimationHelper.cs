namespace AtomicTorch.CBND.CoreMod.ClientComponents.FX
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public static class ClientComponentOneShotSpriteSheetAnimationHelper
    {
        public static void Setup(
            IComponentSpriteRenderer spriteRenderer,
            ITextureAtlasResource textureAtlas,
            double totalDuration)
        {
            Setup(spriteRenderer,
                  ClientComponentSpriteSheetAnimator.CreateAnimationFrames(textureAtlas),
                  totalDuration);
        }

        public static void Setup(
            IComponentSpriteRenderer spriteRenderer,
            ITextureResource[] framesTextureResources,
            double totalDuration)
        {
            var componentAnimator = spriteRenderer.SceneObject
                                                  .AddComponent<ClientComponentSpriteSheetAnimator>();
            componentAnimator.Setup(
                spriteRenderer,
                framesTextureResources,
                frameDurationSeconds: totalDuration / framesTextureResources.Length,
                isLooped: false);

            spriteRenderer.Destroy(totalDuration);
            componentAnimator.Destroy(totalDuration);
        }
    }
}