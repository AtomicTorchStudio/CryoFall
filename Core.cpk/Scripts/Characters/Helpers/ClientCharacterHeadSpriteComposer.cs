namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components.Camera;
    using AtomicTorch.CBND.GameApi.ServicesClient.Rendering;
    using AtomicTorch.GameEngine.Common.Primitives;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public static class ClientCharacterHeadSpriteComposer
    {
        private static readonly Func<string, bool> IsFileExists = Api.Shared.IsFileExists;

        private static readonly IRenderingClientService Rendering = Api.IsClient ? Api.Client.Rendering : null;

        private static readonly ITextureResource TransparentTexturePlaceholder
            = new TextureResource("FX/TransparentPixel");

        public enum HeadSpriteType
        {
            Front,

            Back,

            BackOverlay
        }

        public static async Task<ITextureResource> GenerateHeadSprite(
            CharacterHeadSpriteData data,
            ProceduralTextureRequest request,
            bool isMale,
            HeadSpriteType headSpriteType,
            bool isPreview,
            Vector2Ushort? customTextureSize = null,
            sbyte spriteQualityOffset = 0)
        {
            var isFrontFace = headSpriteType == HeadSpriteType.Front;
            var renderingTag = request.TextureName;
            var side = isFrontFace ? "Front" : "Back";

            var style = data.FaceStyle;

            var faceStylesProvider = SharedCharacterFaceStylesProvider.GetForGender(isMale);

            var facePath = $"{faceStylesProvider.FacesFolderPath}{style.FaceId}/{side}";
            var faceShapePath = facePath + ".png";

            if (!IsFileExists(faceShapePath))
            {
                Api.Logger.Error("Face sprite not found: " + faceShapePath);
                // try fallback
                facePath = faceStylesProvider.FacesFolderPath + "Face01/" + side;
                faceShapePath = facePath;
                if (!IsFileExists(faceShapePath))
                {
                    // no fallback
                    return TextureResource.NoTexture;
                }
            }

            var faceTopPath = $"{facePath}Top{style.TopId}.png";
            var faceBottomPath = $"{facePath}Bottom{style.BottomId}.png";

            if (isFrontFace)
            {
                if (!IsFileExists(faceTopPath))
                {
                    Api.Logger.Error("Face top sprite not found: " + faceTopPath);
                    // try fallback
                    faceTopPath = $"{facePath}Top01.png";
                    if (!IsFileExists(faceTopPath))
                    {
                        // no fallback
                        return TextureResource.NoTexture;
                    }
                }

                if (!IsFileExists(faceBottomPath))
                {
                    Api.Logger.Error("Face bottom sprite not found: " + faceBottomPath);

                    // try fallback
                    faceBottomPath = $"{facePath}Bottom01.png";
                    if (!IsFileExists(faceBottomPath))
                    {
                        // no fallback
                        return TextureResource.NoTexture;
                    }
                }
            }

            var protoItemHeadEquipment = data.HeadEquipmentItemProto;
            var isHairVisible = protoItemHeadEquipment?.IsHairVisible ?? true;
            isHairVisible &= style.HairId is not null;

            string hair = null, hairBehind = null;
            if (isHairVisible
                && !string.IsNullOrEmpty(style.HairId))
            {
                var hairBase = faceStylesProvider.HairFolderPath + $"{style.HairId}/{side}";
                hair = hairBase + ".png";
                hairBehind = hairBase + "Behind.png";
            }

            string skinTone = null;
            if (!string.IsNullOrEmpty(style.SkinToneId))
            {
                skinTone = SharedCharacterFaceStylesProvider.GetSkinToneFilePath(style.SkinToneId);
            }

            string hairColor = null;
            if (!string.IsNullOrEmpty(style.HairColorId))
            {
                hairColor = SharedCharacterFaceStylesProvider.HairColorRootFolderPath + $"{style.HairColorId}" + ".png";
            }

            string helmetFront = null, helmetBehind = null;
            TextureResource helmetFrontMaskTextureResource = null;

            if (protoItemHeadEquipment is not null)
            {
                protoItemHeadEquipment.ClientGetHeadSlotSprites(data.HeadEquipmentItem,
                                                                isMale,
                                                                data.SkeletonResource,
                                                                isFrontFace,
                                                                isPreview,
                                                                out helmetFront,
                                                                out helmetBehind);

                if (helmetFront is null)
                {
                    throw new Exception("Helmet attachment is not available for " + protoItemHeadEquipment);
                }

                if (isFrontFace)
                {
                    helmetFrontMaskTextureResource = new TextureResource(
                        helmetFront.Substring(0, helmetFront.Length - ".png".Length) + "Mask.png",
                        qualityOffset: spriteQualityOffset);

                    if (!Api.Shared.IsFileExists(helmetFrontMaskTextureResource.FullPath))
                    {
                        helmetFrontMaskTextureResource = null;
                    }
                }
            }

            // let's combine all the layers (if some elements are null - they will not be rendered)
            List<ComposeLayer> layers;
            if (protoItemHeadEquipment is null
                || protoItemHeadEquipment.IsHeadVisible)
            {
                var faceLayer = await CreateFaceTexture(
                                    request,
                                    renderingTag,
                                    customTextureSize,
                                    new List<ComposeLayer>()
                                    {
                                        new(faceShapePath, spriteQualityOffset),
                                        new(faceTopPath, spriteQualityOffset),
                                        new(faceBottomPath, spriteQualityOffset)
                                    },
                                    skinTone,
                                    helmetFrontMaskTextureResource);

                layers = new List<ComposeLayer>();

                if (isHairVisible)
                {
                    var (layerHair, layerHairBehind) = await GetHairLayers(request,
                                                                           hair,
                                                                           hairBehind,
                                                                           hairColor,
                                                                           spriteQualityOffset);

                    if (headSpriteType != HeadSpriteType.BackOverlay)
                    {
                        layers.Add(new ComposeLayer(helmetBehind, spriteQualityOffset));
                        layers.Add(layerHairBehind);
                        layers.Add(faceLayer);
                    }

                    if (headSpriteType != HeadSpriteType.Back)
                    {
                        if (layerHair.TextureResource is null
                            && helmetFront is null
                            && layers.Count == 0)
                        {
                            return TransparentTexturePlaceholder;
                        }

                        layers.Add(layerHair);
                        layers.Add(new ComposeLayer(helmetFront, spriteQualityOffset));
                    }
                }
                else // hair is not visible
                {
                    if (headSpriteType == HeadSpriteType.BackOverlay)
                    {
                        return TransparentTexturePlaceholder;
                    }

                    layers.Add(new ComposeLayer(helmetBehind, spriteQualityOffset));
                    layers.Add(faceLayer);
                    layers.Add(new ComposeLayer(helmetFront, spriteQualityOffset));
                }
            }
            else // draw only helmet
            {
                if (headSpriteType == HeadSpriteType.BackOverlay)
                {
                    return TransparentTexturePlaceholder;
                }

                layers = new List<ComposeLayer>()
                {
                    new(helmetBehind, spriteQualityOffset),
                    new(helmetFront, spriteQualityOffset)
                };
            }

            RemoveMissingLayers(layers);

            if (layers.Count == 0)
            {
                Api.Logger.Error("No sprites for face rendering: " + request.TextureName);
                return TextureResource.NoTexture;
            }

            // load all the layers data
            var resultTextureSize = await PrepareLayers(request, layers);
            if (customTextureSize.HasValue)
            {
                resultTextureSize = customTextureSize.Value;
            }

            var renderTexture = await DrawAndDisposeLayers(request, resultTextureSize, renderingTag, layers);
            var generatedTexture = await renderTexture.SaveToTexture(
                                       isTransparent: true,
                                       qualityScaleCoef: Rendering.CalculateCurrentQualityScaleCoefWithOffset(
                                           spriteQualityOffset));
            renderTexture.Dispose();
            request.ThrowIfCancelled();
            return generatedTexture;
        }

        private static async Task<ComposeLayer> CreateFaceTexture(
            ProceduralTextureRequest request,
            string renderingTag,
            Vector2Ushort? customTextureSize,
            List<ComposeLayer> layers,
            string skinToneTextureFilePath,
            TextureResource helmetMaskTextureResource)
        {
            RemoveMissingLayers(layers);
            if (layers.Count == 0)
            {
                return new ComposeLayer(TextureResource.NoTexture);
            }

            var resultTextureSize = await PrepareLayers(request, layers);
            if (customTextureSize.HasValue)
            {
                resultTextureSize = customTextureSize.Value;
            }

            var renderTexture = await DrawAndDisposeLayers(request,
                                                           resultTextureSize,
                                                           renderingTag,
                                                           layers);

            request.ThrowIfCancelled();

            if (helmetMaskTextureResource is not null)
            {
                // apply mask
                var renderTargetForCut = renderTexture;
                try
                {
                    renderTexture = await ClientTextureMaskHelper.ApplyMaskToRenderTargetAsync(request,
                                        renderTexture,
                                        helmetMaskTextureResource);
                }
                finally
                {
                    renderTargetForCut.Dispose();
                }
            }

            if (skinToneTextureFilePath is null)
            {
                return new ComposeLayer(renderTexture);
            }

            try
            {
                var colorizedRenderTexture = await ClientSpriteLutColorRemappingHelper.ApplyColorizerLutAsync(request,
                                                 renderTexture,
                                                 skinToneTextureFilePath);
                return new ComposeLayer(colorizedRenderTexture);
            }
            finally
            {
                renderTexture.Dispose();
            }
        }

        private static async Task<IRenderTarget2D> DrawAndDisposeLayers(
            ProceduralTextureRequest request,
            Vector2Ushort resultTextureSize,
            string renderingTag,
            List<ComposeLayer> layers)
        {
            try
            {
                var referencePivotPos = new Vector2Ushort(
                    (ushort)(resultTextureSize.X / 2),
                    (ushort)(resultTextureSize.Y / 2));

                // create camera and render texture
                var renderTexture =
                    Rendering.CreateRenderTexture(renderingTag, resultTextureSize.X, resultTextureSize.Y);
                var cameraObject = Api.Client.Scene.CreateSceneObject(renderingTag);
                var camera = Rendering.CreateCamera(cameraObject,
                                                    renderingTag,
                                                    drawOrder: -100);
                camera.RenderTarget = renderTexture;
                camera.SetOrthographicProjection(resultTextureSize.X, resultTextureSize.Y);

                // create and prepare renderer for each layer
                foreach (var layer in layers)
                {
                    var pivotPos = layer.PivotPos ?? Vector2F.Zero;
                    var offsetX = referencePivotPos.X - pivotPos.X;
                    var offsetY = pivotPos.Y - referencePivotPos.Y;
                    var offset = (offsetX, offsetY);

                    Rendering.CreateSpriteRenderer(
                        cameraObject,
                        layer.TextureResource,
                        positionOffset: offset,
                        // draw down
                        spritePivotPoint: (0, 1),
                        renderingTag: renderingTag);
                }

                // ReSharper disable once CoVariantArrayConversion
                request.ChangeDependencies(layers.Select(l => l.TextureResource)
                                                 .OfType<TextureResource>()
                                                 .ToArray());

                await camera.DrawAsync();
                cameraObject.Destroy();

                request.ThrowIfCancelled();
                return renderTexture;
            }
            finally
            {
                foreach (var layer in layers)
                {
                    if (layer.TextureResource is IRenderTarget2D renderTarget2D)
                    {
                        renderTarget2D.Dispose();
                    }
                }
            }
        }

        private static async Task<(ComposeLayer layerHair, ComposeLayer layerHairBehind)> GetHairLayers(
            ProceduralTextureRequest request,
            string hair,
            string hairBehind,
            string hairColor,
            sbyte spriteQualityOffset)
        {
            if (hairColor is null)
            {
                // no colorization required
                return (new ComposeLayer(hair,       spriteQualityOffset),
                        new ComposeLayer(hairBehind, spriteQualityOffset));
            }

            // try to colorize the hair
            var layerHairTask = ComposeLayer(hair);
            var layerHairBehindTask = ComposeLayer(hairBehind);

            await Task.WhenAll(layerHairTask, layerHairBehindTask);
            return (layerHairTask.Result, layerHairBehindTask.Result);

            async Task<ComposeLayer> ComposeLayer(string filePath)
            {
                if (filePath is null
                    || !IsFileExists(filePath))
                {
                    return new ComposeLayer(filePath, spriteQualityOffset);
                }

                var textureResource = new TextureResource(filePath,
                                                          isProvidesMagentaPixelPosition: true,
                                                          qualityOffset: spriteQualityOffset);
                var renderTarget = await ClientSpriteLutColorRemappingHelper.ApplyColorizerLutAsync(request,
                                       textureResource,
                                       hairColor);
                var pivotPos = await Rendering.GetTextureSizeWithMagentaPixelPosition(textureResource);

                // mask for hair is not supported yet
                /*if (helmetMaskTextureResource is not null)
                {
                    var renderTargetForCut = renderTarget;
                    try
                    {
                        renderTarget = await ClientTextureMaskHelper.ApplyMaskToRenderTargetAsync(request,
                                           renderTargetForCut,
                                           helmetMaskTextureResource);
                    }
                    finally
                    {
                        renderTargetForCut.Dispose();
                    }
                }*/

                return new ComposeLayer(renderTarget,
                                        (pivotPos.MagentaPixelPosition.X,
                                         pivotPos.MagentaPixelPosition.Y));
            }
        }

        private static Task<SizeWithMagentaPixelPosition> GetTextureSizeWithMagentaPixelPosition(
            ComposeLayer layer)
        {
            switch (layer.TextureResource)
            {
                case TextureResource textureResource:
                    return Rendering.GetTextureSizeWithMagentaPixelPosition(
                        textureResource).AsTask();

                case IGeneratedTexture2D generatedTexture2D:
                {
                    var textureSize = new Vector2Ushort((ushort)generatedTexture2D.Width,
                                                        (ushort)generatedTexture2D.Height);
                    return Task.FromResult(
                        new SizeWithMagentaPixelPosition(
                            textureSize,
                            layer.PivotPos ?? (textureSize.X / 2, textureSize.Y / 2)));
                }

                case IRenderTarget2D renderTarget2D:
                {
                    var textureSize = new Vector2Ushort((ushort)renderTarget2D.Width,
                                                        (ushort)renderTarget2D.Height);
                    return Task.FromResult(
                        new SizeWithMagentaPixelPosition(
                            textureSize,
                            layer.PivotPos ?? (textureSize.X / 2, textureSize.Y / 2)));
                }

                default:
                    throw new NotImplementedException();
            }
        }

        private static async ValueTask<Vector2Ushort> PrepareLayers(
            ProceduralTextureRequest request,
            List<ComposeLayer> spritesToCombine)
        {
            var textureDataTasks = new Task<SizeWithMagentaPixelPosition>[spritesToCombine.Count];
            for (var index = 0; index < spritesToCombine.Count; index++)
            {
                var layer = spritesToCombine[index];
                var task = GetTextureSizeWithMagentaPixelPosition(layer);
                textureDataTasks[index] = task;
            }

            await Task.WhenAll(textureDataTasks);

            request.ThrowIfCancelled();

            var extendX = 0.0;
            var extendY = 0.0;

            for (var index = 0; index < textureDataTasks.Length; index++)
            {
                var result = textureDataTasks[index].Result;
                var pivotPos = result.MagentaPixelPosition;
                var composeItem = spritesToCombine[index];
                composeItem.PivotPos = pivotPos;
                spritesToCombine[index] = composeItem;

                var itemExtendX = Math.Max(pivotPos.X, result.Size.X - pivotPos.X);
                var itemExtendY = Math.Max(pivotPos.Y, result.Size.Y - pivotPos.Y);
                if (itemExtendX > extendX)
                {
                    extendX = itemExtendX;
                }

                if (itemExtendY > extendY)
                {
                    extendY = itemExtendY;
                }
            }

            return new Vector2Ushort((ushort)Math.Floor(extendX * 2),
                                     (ushort)Math.Floor(extendY * 2));
        }

        private static void RemoveMissingLayers(List<ComposeLayer> layers)
        {
            layers.RemoveAll(IsMissing);

            static bool IsMissing(ComposeLayer layer)
            {
                var textureResource = layer.TextureResource;
                if (textureResource is null)
                {
                    return true;
                }

                return textureResource is TextureResource textureFileResource
                       && !IsFileExists(textureFileResource.FullPath);
            }
        }

        private struct ComposeLayer
        {
            public readonly ITextureResource TextureResource;

            public Vector2F? PivotPos;

            public ComposeLayer(string path, sbyte spriteQualityOffset)
            {
                this.TextureResource = path is not null
                                           ? new TextureResource(
                                               path.Substring(ContentPaths.Textures.Length),
                                               isProvidesMagentaPixelPosition: true,
                                               qualityOffset: spriteQualityOffset)
                                           : null;
                this.PivotPos = default;
            }

            public ComposeLayer(ITextureResource textureResource, Vector2F? pivotPos = default)
            {
                this.TextureResource = textureResource;
                this.PivotPos = pivotPos;
            }
        }
    }
}