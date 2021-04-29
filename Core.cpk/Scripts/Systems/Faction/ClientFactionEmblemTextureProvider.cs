namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using static SharedFactionEmblemProvider;

    public static class ClientFactionEmblemTextureProvider
    {
        public const sbyte SpriteQualityOffset = -1;

        private static readonly ushort EmblemTextureSize
            = (ushort)(256 / Api.Client.Rendering.CalculateCurrentQualityScaleCoefWithOffset(SpriteQualityOffset));

        private static readonly EffectResource EffectResourceDrawWithMask
            = new("DrawWithMask");

        private static readonly Dictionary<string, ProceduralTexture> EmblemProceduralTextureCache
            = new();

        private static readonly RenderingMaterial RenderingMaterialForeground
            = RenderingMaterial.Create(
                new EffectResource("Special/FactionEmblemForeground"));

        public static ITextureResource GetEmblemTexture(ILogicObject faction, bool useCache)
        {
            var factionPublicState = Faction.GetPublicState(faction);
            var emblem = factionPublicState.Emblem;
            return GetEmblemTexture(emblem, useCache);
        }

        public static ITextureResource GetEmblemTexture(FactionEmblem emblem, bool useCache)
        {
            var emblemId = GetEmblemId(emblem);
            if (useCache
                && EmblemProceduralTextureCache.TryGetValue(emblemId, out var proceduralTexture))
            {
                return proceduralTexture;
            }

            proceduralTexture = new ProceduralTexture(emblemId,
                                                      GenerateEmblemTexture,
                                                      isTransparent: true,
                                                      isUseCache: false,
                                                      data: emblem);

            if (useCache)
            {
                EmblemProceduralTextureCache[emblemId] = proceduralTexture;
            }

            return proceduralTexture;
        }

        public static async Task<ITextureResource> GetEmblemTextureAsync(string clanTag, bool useCache)
        {
            var emblem = await ClientFactionEmblemDataCache.GetFactionEmblemAsync(clanTag);
            return GetEmblemTexture(emblem, useCache);
        }

        private static async Task<ITextureResource> GenerateEmblemTexture(ProceduralTextureRequest request)
        {
            var emblem = (FactionEmblem)request.Data;
            if (!SharedIsValidEmblem(emblem))
            {
                var message = "Invalid faction emblem: " + request.TextureName;
                if (Api.IsEditor)
                {
                    Api.Logger.Error(message);
                }
                else
                {
                    Api.Logger.Warning(message);
                }

                return await GeneratePlaceholderTexture(request);
            }

            var textureResources = new (ITextureResource TextureResource, Color Color)[]
            {
                (new TextureResource(GetBackgroundFilePath(emblem.BackgroundId),
                                     qualityOffset: SpriteQualityOffset),
                 emblem.ColorBackground2),

                (new TextureResource(GetForegroundFilePath(emblem.ForegroundId),
                                     qualityOffset: SpriteQualityOffset),
                 emblem.ColorForeground)
            };

            var shapeMaskTextureResource = new TextureResource(GetShapeMaskFilePath(emblem.ShapeMaskId),
                                                               qualityOffset: SpriteQualityOffset);

            var rendering = Api.Client.Rendering;
            var renderingTag = request.TextureName;

            // create camera and render texture
            var renderTexture1 = rendering.CreateRenderTexture(renderingTag, EmblemTextureSize, EmblemTextureSize);
            var renderTexture2 = rendering.CreateRenderTexture(renderingTag, EmblemTextureSize, EmblemTextureSize);
            var cameraObject = Api.Client.Scene.CreateSceneObject(renderingTag);
            var camera = rendering.CreateCamera(cameraObject,
                                                renderingTag,
                                                drawOrder: -100);
            camera.ClearColor = emblem.ColorBackground1;
            camera.RenderTarget = renderTexture1;
            camera.SetOrthographicProjection(EmblemTextureSize, EmblemTextureSize);

            // draw the emblem layers (without the mask) into the first render target
            var layerRenderers = new IComponentSpriteRenderer[textureResources.Length];
            for (var index = 0; index < textureResources.Length; index++)
            {
                var entry = textureResources[index];
                var renderer = rendering.CreateSpriteRenderer(
                    cameraObject,
                    entry.TextureResource,
                    spritePivotPoint: (0, 1),
                    renderingTag: renderingTag);
                renderer.Color = entry.Color;
                renderer.Size = EmblemTextureSize / 2.0;
                layerRenderers[index] = renderer;

                if (index == textureResources.Length - 1)
                {
                    renderer.RenderingMaterial = RenderingMaterialForeground;
                }
            }

            await camera.DrawAsync();
            if (request.CancellationToken.IsCancellationRequested)
            {
                cameraObject.Destroy();
                request.ThrowIfCancelled();
            }

            foreach (var renderer in layerRenderers)
            {
                renderer.Destroy();
            }

            layerRenderers = null;

            // draw the first render target into the second with a mask
            {
                var maskRenderer = rendering.CreateSpriteRenderer(
                    cameraObject,
                    renderTexture1,
                    spritePivotPoint: (0, 1),
                    renderingTag: renderingTag);
                var material = RenderingMaterial.Create(EffectResourceDrawWithMask);
                material.EffectParameters.Set("MaskTextureArray", shapeMaskTextureResource);
                maskRenderer.RenderingMaterial = material;
            }

            camera.ClearColor = Color.FromArgb(0, 0, 0, 0);
            camera.RenderTarget = renderTexture2;
            await camera.DrawAsync();
            cameraObject.Destroy();

            request.ThrowIfCancelled();

            var generatedTexture = await renderTexture2.SaveToTexture(
                                       isTransparent: false,
                                       qualityScaleCoef: rendering.CalculateCurrentQualityScaleCoefWithOffset(
                                           SpriteQualityOffset));
            renderTexture1.Dispose();
            renderTexture2.Dispose();
            request.ThrowIfCancelled();
            return generatedTexture;
        }

        private static async Task<ITextureResource> GeneratePlaceholderTexture(ProceduralTextureRequest request)
        {
            var rendering = Api.Client.Rendering;
            var renderingTag = request.TextureName;
            var renderTexture = rendering.CreateRenderTexture(renderingTag, EmblemTextureSize, EmblemTextureSize);
            var cameraObject = Api.Client.Scene.CreateSceneObject(renderingTag);
            var camera = rendering.CreateCamera(cameraObject,
                                                renderingTag,
                                                drawOrder: -100);
            camera.ClearColor = Colors.Magenta;
            camera.RenderTarget = renderTexture;
            await camera.DrawAsync();
            cameraObject.Destroy();

            request.ThrowIfCancelled();

            var generatedTexture = await renderTexture.SaveToTexture(isTransparent: false);
            renderTexture.Dispose();
            request.ThrowIfCancelled();
            return generatedTexture;
        }

        private static string GetEmblemId(FactionEmblem emblem)
        {
            return string.Format("{0};{1};{2};{3};{4};{5}",
                                 emblem.ForegroundId,
                                 emblem.BackgroundId,
                                 emblem.ShapeMaskId,
                                 emblem.ColorForeground,
                                 emblem.ColorBackground1,
                                 emblem.ColorBackground2);
        }
    }
}