namespace AtomicTorch.CBND.CoreMod
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientTextureMaskHelper
    {
        private static readonly IRenderingClientService Renderer = Api.Client.Rendering;

        public static async Task<IRenderTarget2D> ApplyMaskToRenderTargetAsync(
            ProceduralTextureRequest request,
            IRenderTarget2D sourceRenderTarget,
            TextureResource maskResource)
        {
            var textureWidth = (ushort)sourceRenderTarget.Width;
            var textureHeight = (ushort)sourceRenderTarget.Height;
            var maskSizeAbsolute = await Renderer.GetTextureSize(maskResource);

            var maskScale = new Vector2F((float)(textureWidth / (double)maskSizeAbsolute.X),
                                         (float)(textureHeight / (double)maskSizeAbsolute.Y));

            var maskOffset = new Vector2F((float)((maskSizeAbsolute.X - textureWidth) / (2.0 * textureWidth)),
                                          (float)((maskSizeAbsolute.Y - textureHeight) / (2.0 * textureHeight)));

            /*Api.Logger.Dev(
                string.Format("Texture size: X={0} Y={1}"
                              + "{2}Mask size: X={3} Y={4}"
                              + "{2}Mask scale: X={5:F2} Y={6:F2}"
                              + "{2}Mask offset: X={7:F2} Y={8:F2}",
                              textureWidth,
                              textureHeight,
                              Environment.NewLine,
                              maskSizeAbsolute.X,
                              maskSizeAbsolute.Y,
                              maskScale.X,
                              maskScale.Y,
                              maskOffset.X,
                              maskOffset.Y));*/

            var renderingMaterial = RenderingMaterial.Create(new EffectResource("DrawWithMaskOffset"));
            renderingMaterial.EffectParameters
                             .Set("MaskTextureArray", maskResource)
                             .Set("MaskScale",        maskScale)
                             .Set("MaskOffset",       maskOffset);

            var renderingTag = "Mask camera for procedural texture: " + request.TextureName;
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
                                          sourceRenderTarget,
                                          renderingTag: renderingTag,
                                          // draw down
                                          spritePivotPoint: (0, 1))
                    .RenderingMaterial = renderingMaterial;

            await camera.DrawAsync();

            cameraObject.Destroy();
            request.ThrowIfCancelled();

            return renderTarget;
        }
    }
}