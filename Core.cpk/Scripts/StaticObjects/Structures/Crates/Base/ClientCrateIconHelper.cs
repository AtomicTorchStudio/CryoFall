namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientCrateIconHelper
    {
        private static readonly Dictionary<ITextureResource, WeakReference<ProceduralTexture>> IconsCache
            = new();

        private static readonly IRenderingClientService Rendering
            = Api.Client.Rendering;

        private static readonly RenderingMaterial RenderingMaterialCrateIcon;

        static ClientCrateIconHelper()
        {
            RenderingMaterialCrateIcon = RenderingMaterial.Create(
                new EffectResource("Special/CrateIcon"));
            RenderingMaterialCrateIcon.EffectParameters
                                      .Set("MaskTexture", new TextureResource("FX/Special/CrateIconMask"));
        }

        public static ITextureResource GetIcon(IProtoEntity protoEntity)
        {
            if (protoEntity is null)
            {
                return null;
            }

            var originalIcon = GetOriginalIcon(protoEntity);
            if (IconsCache.TryGetValue(originalIcon, out var weakReference)
                && weakReference.TryGetTarget(out var proceduralTexture))
            {
                return proceduralTexture;
            }

            proceduralTexture = new ProceduralTexture(
                "Crate icon " + protoEntity.ShortId,
                proceduralTextureRequest => GenerateIcon(proceduralTextureRequest, originalIcon),
                isTransparent: true,
                isUseCache: true,
                dependsOn: new[] { originalIcon });
            IconsCache[originalIcon] = new WeakReference<ProceduralTexture>(proceduralTexture);
            return proceduralTexture;
        }

        private static async Task<ITextureResource> GenerateIcon(
            ProceduralTextureRequest request,
            ITextureResource originalIcon)
        {
            Vector2Ushort size = (128, 128);

            // expand size a bit to fit the outline
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
                // as most icons are 256x256 and we need a 128x128 icon scale it this way
                scale: 0.5);

            spriteRenderer.RenderingMaterial = RenderingMaterialCrateIcon;

            await camera.DrawAsync();
            cameraObject.Destroy();

            request.ThrowIfCancelled();

            var generatedTexture = await renderTexture.SaveToTexture(isTransparent: true);
            renderTexture.Dispose();
            request.ThrowIfCancelled();
            return generatedTexture;
        }

        public static ITextureResource GetOriginalIcon(IProtoEntity protoEntity)
        {
            switch (protoEntity)
            {
                case IProtoItem protoItem:
                {
                    return protoItem.Icon;
                }

                case IProtoCharacterMob protoCharacterMob:
                {
                    return protoCharacterMob.Icon;
                }

                case TechGroup techGroup:
                {
                    return techGroup.Icon;
                }

                default:
                {
                    return null;
                }
            }
        }
    }
}