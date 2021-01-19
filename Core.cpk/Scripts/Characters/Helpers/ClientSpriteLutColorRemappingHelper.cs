namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System.Threading.Tasks;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;

    public static class ClientSpriteLutColorRemappingHelper
    {
        private static readonly IRenderingClientService Renderer = Api.Client.Rendering;

        public static async Task<IRenderTarget2D> ApplyColorizerLut(
            ProceduralTextureRequest request,
            ITextureResource sourceTextureResource,
            string lutTextureFilePath)
        {
            var lutTextureResource = new TextureResource3D(lutTextureFilePath, depth: 24, isTransparent: false);
            var lutTexture3D = await Renderer.LoadTexture3D(lutTextureResource);

            var textureSize = await Renderer.GetTextureSize(sourceTextureResource);
            var textureWidth = textureSize.X;
            var textureHeight = textureSize.Y;

            var renderingMaterial = RenderingMaterial.Create(new EffectResource("ColorLutRemap"));
            renderingMaterial.EffectParameters.Set("TextureLut", lutTexture3D);

            var renderingTag = "Colorizer camera for procedural texture: " + request.TextureName;
            var cameraObject = Api.Client.Scene.CreateSceneObject(renderingTag);
            var camera = Renderer.CreateCamera(cameraObject,
                                               renderingTag: renderingTag,
                                               drawOrder: -10,
                                               drawMode: CameraDrawMode.Manual);

            var renderTarget = Renderer.CreateRenderTexture(renderingTag, textureWidth, textureHeight);
            camera.RenderTarget = renderTarget;
            camera.ClearColor = Color.FromArgb(0, 0, 0, 0);
            camera.SetOrthographicProjection(textureWidth, textureHeight);

            Renderer.CreateSpriteRenderer(cameraObject,
                                          sourceTextureResource,
                                          renderingTag: renderingTag,
                                          // draw down
                                          spritePivotPoint: (0, 1))
                    .RenderingMaterial = renderingMaterial;

            await camera.DrawAsync();

            cameraObject.Destroy();
            request.ThrowIfCancelled();

            return renderTarget;
        }

        public static async Task<ITextureResource> GetColorizedSprite(
            ProceduralTextureRequest request,
            ITextureResource textureResource,
            string lutTextureFilePath,
            sbyte spriteQualityOffset)
        {
            var renderTarget2D = await ApplyColorizerLut(request,
                                                         textureResource,
                                                         lutTextureFilePath);

            var result = await renderTarget2D.SaveToTexture(
                             isTransparent: true,
                             qualityScaleCoef: Renderer.CalculateCurrentQualityScaleCoefWithOffset(
                                 spriteQualityOffset),
                             customName: request.TextureName);

            renderTarget2D.Dispose();

            return result;
        }
    }
}