namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public static class ClientCharacterHeadSpriteComposer
    {
        private static readonly Func<string, bool> IsFileExists = Api.Shared.IsFileExists;

        private static readonly IRenderingClientService Rendering = Api.IsClient ? Api.Client.Rendering : null;

        public static async Task<ITextureResource> GenerateHeadSprite(
            CharacterHeadSpriteData data,
            ProceduralTextureRequest request,
            bool isMale,
            bool isFrontFace,
            Vector2Ushort? customTextureSize = null,
            sbyte spriteQualityOffset = 0)
        {
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

            var itemHeadEquipment = data.HeadEquipment;
            var protoItemHeadEquipment = (IProtoItemEquipmentHead)itemHeadEquipment?.ProtoItem;
            var isHairVisible = protoItemHeadEquipment?.IsHairVisible ?? true;
            isHairVisible &= style.HairId != null;

            string hair = null, hairBehind = null;
            if (isHairVisible)
            {
                var hairBase = faceStylesProvider.HairFolderPath + $"{style.HairId}/{side}";
                hair = hairBase + ".png";
                hairBehind = hairBase + "Behind.png";
            }

            string helmetFront = null, helmetBehind = null;
            if (protoItemHeadEquipment != null)
            {
                GetHeadEquipmentSprites(itemHeadEquipment,
                                        protoItemHeadEquipment,
                                        isMale,
                                        isFrontFace,
                                        data.SkeletonResource,
                                        out helmetFront,
                                        out helmetBehind);
            }

            // let's combine all the layers (if some elements are null - they will not be rendered)
            var layers = new List<ComposeLayer>()
            {
                new ComposeLayer(helmetBehind,   spriteQualityOffset),
                new ComposeLayer(hairBehind,     spriteQualityOffset),
                new ComposeLayer(faceShapePath,  spriteQualityOffset),
                new ComposeLayer(faceTopPath,    spriteQualityOffset),
                new ComposeLayer(faceBottomPath, spriteQualityOffset),
                new ComposeLayer(hair,           spriteQualityOffset),
                new ComposeLayer(helmetFront,    spriteQualityOffset),
            };

            // load only those layers which had the according file
            layers.RemoveAll(
                t => t.TextureResource == null
                     || !IsFileExists(t.TextureResource.FullPath));

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

            var referencePivotPos = new Vector2Ushort(
                (ushort)(resultTextureSize.X / 2),
                (ushort)(resultTextureSize.Y / 2));

            // create camera and render texture
            var renderTexture = Rendering.CreateRenderTexture(renderingTag, resultTextureSize.X, resultTextureSize.Y);
            var cameraObject = Api.Client.Scene.CreateSceneObject(renderingTag);
            var camera = Rendering.CreateCamera(cameraObject,
                                                renderingTag,
                                                drawOrder: -100);
            camera.RenderTarget = renderTexture;
            camera.SetOrthographicProjection(resultTextureSize.X, resultTextureSize.Y);

            // create and prepare renderer for each layer
            foreach (var layer in layers)
            {
                var pivotPos = layer.PivotPos;
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
            request.ChangeDependencies(layers.Select(l => l.TextureResource).ToArray());

            await camera.DrawAsync();
            cameraObject.Destroy();

            request.ThrowIfCancelled();

            var generatedTexture = await renderTexture.SaveToTexture(
                                       isTransparent: true,
                                       qualityScaleCoef: Rendering.CalculateCurrentQualityScaleCoefWithOffset(
                                           spriteQualityOffset));
            renderTexture.Dispose();
            request.ThrowIfCancelled();
            return generatedTexture;
        }

        private static void GetHeadEquipmentSprites(
            IItem itemHeadEquipment,
            IProtoItemEquipmentHead protoHelmet,
            bool isMale,
            bool isFrontFace,
            SkeletonResource skeletonResource,
            out string helmetFront,
            out string helmetBehind)
        {
            helmetFront = helmetBehind = null;
            protoHelmet.ClientGetHeadSlotSprites(
                itemHeadEquipment,
                isMale,
                skeletonResource,
                isFrontFace,
                out helmetFront,
                out helmetBehind);

            if (helmetFront == null)
            {
                throw new Exception("Helmet attachment is not available for " + protoHelmet);
            }
        }

        private static async ValueTask<Vector2Ushort> PrepareLayers(
            ProceduralTextureRequest request,
            List<ComposeLayer> spritesToCombine)
        {
            var textureDataTasks = spritesToCombine.Select(
                                                       t => Rendering
                                                           .GetTextureSizeWithMagentaPixelPosition(t.TextureResource))
                                                   .ToList();

            foreach (var textureDataTask in textureDataTasks)
            {
                await textureDataTask;
                request.ThrowIfCancelled();
            }

            var extendX = 0f;
            var extendY = 0f;

            for (var index = 0; index < textureDataTasks.Count; index++)
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

            var resultTextureSize = new Vector2Ushort((ushort)Math.Floor(extendX * 2),
                                                      (ushort)Math.Floor(extendY * 2));
            return resultTextureSize;
        }

        private struct ComposeLayer
        {
            public readonly TextureResource TextureResource;

            public Vector2F PivotPos;

            public ComposeLayer(string path, sbyte spriteQualityOffset)
            {
                this.TextureResource = path != null
                                           ? new TextureResource(
                                               path.Substring(ContentPaths.Textures.Length),
                                               isProvidesMagentaPixelPosition: true,
                                               qualityOffset: spriteQualityOffset)
                                           : null;
                this.PivotPos = Vector2F.Zero;
            }
        }
    }
}