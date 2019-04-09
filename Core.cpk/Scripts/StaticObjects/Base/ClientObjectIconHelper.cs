namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using System;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientObjectIconHelper
    {
        private static readonly IRenderingClientService Rendering
            = Api.Client.Rendering;

        private static int MaxIconTextureSIze
            => 512 / Rendering.SpriteQualitySizeMultiplierReverse;

        public static async Task<ITextureResource> CreateIcon(
            ITextureResource texture,
            ProceduralTextureRequest request)
        {
            var textureSize = await Rendering.GetTextureSize(texture);
            request.ThrowIfCancelled();

            if (!ClampTextureSize(ref textureSize, out var scale)
                && texture is TextureResource)
            {
                // can use original texture as icon for itself
                return texture;
            }

            // create camera and render texture
            var renderingTag = request.TextureName;
            var renderTexture = Rendering.CreateRenderTexture(renderingTag,
                                                              textureSize.X,
                                                              textureSize.Y);
            var cameraObject = Api.Client.Scene.CreateSceneObject(renderingTag);
            var camera = Rendering.CreateCamera(cameraObject,
                                                renderingTag,
                                                drawOrder: -100);
            camera.RenderTarget = renderTexture;
            camera.SetOrthographicProjection(textureSize.X, textureSize.Y);

            // create and prepare renderer for icon (attach it to camera object)
            Rendering.CreateSpriteRenderer(
                cameraObject,
                texture,
                positionOffset: (0, 0),
                // draw down
                spritePivotPoint: (0, 1),
                renderingTag: renderingTag,
                scale: scale);

            await camera.DrawAsync();
            cameraObject.Destroy();

            request.ThrowIfCancelled();

            var generatedTexture = await renderTexture.SaveToTexture(isTransparent: true);
            renderTexture.Dispose();
            request.ThrowIfCancelled();
            return generatedTexture;
        }

        private static bool ClampTextureSize(ref Vector2Ushort textureSize, out double scale)
        {
            var size = Math.Max(textureSize.X, textureSize.Y);
            var maxSize = MaxIconTextureSIze;
            if (size < maxSize)
            {
                scale = 1;
                return false;
            }

            scale = Math.Min(maxSize / (double)textureSize.X,
                             maxSize / (double)textureSize.Y);

            textureSize = new Vector2Ushort((ushort)(textureSize.X * scale),
                                            (ushort)(textureSize.Y * scale));
            return true;
        }
    }
}