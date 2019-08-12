namespace AtomicTorch.CBND.CoreMod.Items
{
    using System.Threading.Tasks;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public static class ClientItemIconHelper
    {
        private static readonly IRenderingClientService Rendering
            = Api.IsClient
                  ? Api.Client.Rendering
                  : null;

        public static ProceduralTexture CreateComposedIcon(
            string name,
            ITextureResource primaryIcon,
            ITextureResource secondaryIcon)
        {
            if (Api.IsServer)
            {
                return null;
            }

            return new ProceduralTexture(
                name,
                generateTextureCallback: GenerateTexture,
                isTransparent: true,
                isUseCache: true,
                dependsOn: new[] { primaryIcon, secondaryIcon });

            async Task<ITextureResource> GenerateTexture(ProceduralTextureRequest request)
            {
                var size = await Rendering.GetTextureSize(primaryIcon);
                request.ThrowIfCancelled();

                // create camera and render texture
                var renderingTag = request.TextureName;
                var renderTexture = Rendering.CreateRenderTexture(renderingTag,
                                                                  size.X,
                                                                  size.Y);
                var cameraObject = Api.Client.Scene.CreateSceneObject(renderingTag);
                var camera = Rendering.CreateCamera(cameraObject,
                                                    renderingTag,
                                                    drawOrder: -100);
                camera.RenderTarget = renderTexture;
                camera.SetOrthographicProjection(size.X, size.Y);

                Rendering.CreateSpriteRenderer(
                    cameraObject,
                    primaryIcon,
                    positionOffset: (0, 0),
                    // draw down
                    spritePivotPoint: (0, 1),
                    renderingTag: renderingTag,
                    scale: 1);

                var secondaryIconScale = 0.6;
                Rendering.CreateSpriteRenderer(
                    cameraObject,
                    secondaryIcon,
                    positionOffset: (size.X * (1 - secondaryIconScale),
                                     -size.Y * (1 - secondaryIconScale)),
                    // draw down
                    spritePivotPoint: (0, 1),
                    renderingTag: renderingTag,
                    scale: secondaryIconScale);

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
}