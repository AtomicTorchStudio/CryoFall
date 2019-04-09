namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using System.Threading.Tasks;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientProceduralTextureHelper
    {
        public static ProceduralTexture CreateComposedTexture(
            string name,
            bool isTransparent,
            bool isUseCache,
            params ITextureResource[] textureResources)
        {
            return new ProceduralTexture(
                name,
                generateTextureCallback: request => Compose(request, textureResources),
                isTransparent: isTransparent,
                isUseCache: isUseCache,
                dependsOn: textureResources);
        }

        private static async Task<ITextureResource> Compose(
            ProceduralTextureRequest request,
            params ITextureResource[] textureResources)
        {
            var rendering = Api.Client.Rendering;
            var renderingTag = request.TextureName;

            var textureSize = await rendering.GetTextureSize(textureResources[0]);
            request.ThrowIfCancelled();

            // create camera and render texture
            var renderTexture = rendering.CreateRenderTexture(renderingTag, textureSize.X, textureSize.Y);
            var cameraObject = Api.Client.Scene.CreateSceneObject(renderingTag);
            var camera = rendering.CreateCamera(cameraObject,
                                                renderingTag,
                                                drawOrder: -100);
            camera.RenderTarget = renderTexture;
            camera.SetOrthographicProjection(textureSize.X, textureSize.Y);

            // create and prepare sprite renderers
            foreach (var textureResource in textureResources)
            {
                rendering.CreateSpriteRenderer(
                    cameraObject,
                    textureResource,
                    positionOffset: (0, 0),
                    // draw down
                    spritePivotPoint: (0, 1),
                    renderingTag: renderingTag);
            }

            await camera.DrawAsync();
            cameraObject.Destroy();

            request.ThrowIfCancelled();

            var generatedTexture = await renderTexture.SaveToTexture(isTransparent: true);
            renderTexture.Dispose();
            request.ThrowIfCancelled();
            return generatedTexture;
        }
    }
}