namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Completionist.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    internal static class CompletionistLockedEntryIconHelper
    {
        private static readonly IRenderingClientService Rendering
            = Api.Client.Rendering;

        private static readonly Dictionary<float, RenderingMaterial> RenderingMaterialLockedEntryByOutlineSize
            = new();

        public static async Task<ITextureResource> CreateIconForLockedEntry(
            ProceduralTextureRequest request,
            ITextureResource originalIcon)
        {
            var size = await Rendering.GetTextureSize(originalIcon);
            var originalIconSize = size;

            // expand size a bit to fit the outline
            var paddingFraction = 0.0;
            var padding = (int)Math.Round(size.X * paddingFraction, MidpointRounding.AwayFromZero);
            size = new Vector2Ushort((ushort)(size.X + padding),
                                     (ushort)(size.Y + padding));
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

            var spriteRenderer = Rendering.CreateSpriteRenderer(
                cameraObject,
                originalIcon,
                // draw at the center
                positionOffset: (size.X / 2, -size.Y / 2),
                spritePivotPoint: (0.5, 0.5),
                renderingTag: renderingTag,
                scale: 1 + paddingFraction);

            spriteRenderer.RenderingMaterial = GetRenderingMaterial(originalIconSize);

            await camera.DrawAsync();
            cameraObject.Destroy();

            request.ThrowIfCancelled();

            var generatedTexture = await renderTexture.SaveToTexture(isTransparent: true);
            renderTexture.Dispose();
            request.ThrowIfCancelled();
            return generatedTexture;
        }

        private static RenderingMaterial GetRenderingMaterial(Vector2Ushort size)
        {
            var outlineSize = (float)(0.02 * 256.0 / size.X);
            if (RenderingMaterialLockedEntryByOutlineSize.TryGetValue(outlineSize, out var result))
            {
                return result;
            }

            result = RenderingMaterial.Create(
                new EffectResource("Special/CompletionistLockedEntryIcon"));
            result.EffectParameters.Set("OutlineSize", outlineSize);
            RenderingMaterialLockedEntryByOutlineSize[outlineSize] = result;
            return result;
        }
    }
}