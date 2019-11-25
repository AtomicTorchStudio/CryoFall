namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using System.Threading.Tasks;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientProceduralTextureHelper
    {
        public static ProceduralTexture CreateComposedTexture(
            string name,
            bool isTransparent,
            bool isUseCache,
            Vector2Ushort? customSize = null,
            params TextureResourceWithOffset[] textureResourcesWithOffsets)
        {
            var textureResources = new ITextureResource[textureResourcesWithOffsets.Length];
            for (var index = 0; index < textureResourcesWithOffsets.Length; index++)
            {
                var textureResourceWithOffset = textureResourcesWithOffsets[index];
                textureResources[index] = textureResourceWithOffset.TextureResource;
            }

            return CreateComposedTextureInternal(name,
                                                 isTransparent,
                                                 isUseCache,
                                                 textureResourcesWithOffsets,
                                                 textureResources,
                                                 customSize);
        }

        public static ProceduralTexture CreateComposedTexture(
            string name,
            bool isTransparent,
            bool isUseCache,
            Vector2Ushort? customSize = null,
            params ITextureResource[] textureResources)
        {
            var textureResourcesWithOffsets = new TextureResourceWithOffset[textureResources.Length];
            for (var index = 0; index < textureResources.Length; index++)
            {
                var textureResource = textureResources[index];
                textureResourcesWithOffsets[index] = new TextureResourceWithOffset(textureResource);
            }

            return CreateComposedTextureInternal(name,
                                                 isTransparent,
                                                 isUseCache,
                                                 textureResourcesWithOffsets,
                                                 textureResources,
                                                 customSize);
        }

        private static async Task<ITextureResource> Compose(
            ProceduralTextureRequest request,
            Vector2Ushort? customSize,
            params TextureResourceWithOffset[] textureResources)
        {
            var rendering = Api.Client.Rendering;
            var renderingTag = request.TextureName;

            var size = customSize ?? await rendering.GetTextureSize(textureResources[0].TextureResource);
            request.ThrowIfCancelled();

            // create camera and render texture
            var renderTexture = rendering.CreateRenderTexture(renderingTag, size.X, size.Y);
            var cameraObject = Api.Client.Scene.CreateSceneObject(renderingTag);
            var camera = rendering.CreateCamera(cameraObject,
                                                renderingTag,
                                                drawOrder: -100);
            camera.RenderTarget = renderTexture;
            camera.SetOrthographicProjection(size.X, size.Y);

            // create and prepare sprite renderers
            foreach (var entry in textureResources)
            {
                rendering.CreateSpriteRenderer(
                    cameraObject,
                    entry.TextureResource,
                    positionOffset: entry.Offset,
                    spritePivotPoint: entry.PivotPoint ?? (0, 1), // draw down by default
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

        private static ProceduralTexture CreateComposedTextureInternal(
            string name,
            bool isTransparent,
            bool isUseCache,
            TextureResourceWithOffset[] textureResources,
            ITextureResource[] textureResourcesWithoutOffets,
            Vector2Ushort? customSize)
        {
            return new ProceduralTexture(
                name,
                generateTextureCallback: request => Compose(request, customSize, textureResources),
                isTransparent: isTransparent,
                isUseCache: isUseCache,
                dependsOn: textureResourcesWithoutOffets);
        }
    }
}