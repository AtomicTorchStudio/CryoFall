namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using static TileBlendSides;

    public static class ClientTileBlendHelper
    {
        public const ushort TextureMaskCornerInnerArraySlice = 3;

        public const ushort TextureMaskCornerOuterArraySlice = 0;

        public const ushort TextureMaskHorizontalArraySlice = 1;

        public const ushort TextureMaskVerticalArraySlice = 2;

        private static readonly Dictionary<MaterialCacheKey, RenderingMaterial> CachedBlendMaskMaterials
            = new Dictionary<MaterialCacheKey, RenderingMaterial>();

        private static readonly IReadOnlyList<EffectResource> EffectForMaskCounts
            = new[]
            {
                new EffectResource("Terrain/GroundTileBlendOneMaskLayer.fx"),
                new EffectResource("Terrain/GroundTileBlendTwoMaskLayers.fx"),
                new EffectResource("Terrain/GroundTileBlendThreeMaskLayers.fx"),
                new EffectResource("Terrain/GroundTileBlendFourMaskLayers.fx")
            };

        private static readonly IClientStorage SessionStorage;

        private static bool isBlendingEnabled;

        static ClientTileBlendHelper()
        {
            SessionStorage = Api.Client.Storage.GetSessionStorage(
                $"{nameof(ClientTileBlendHelper)}.{nameof(IsBlendingEnabled)}");
            if (!SessionStorage.TryLoad(out isBlendingEnabled))
            {
                isBlendingEnabled = true;
            }
        }

        public static event Action IsBlendingEnabledChanged;

        public static bool IsBlendingEnabled
        {
            get => isBlendingEnabled;
            set
            {
                if (isBlendingEnabled == value)
                {
                    return;
                }

                isBlendingEnabled = value;
                SessionStorage.Save(isBlendingEnabled);
                Api.Client.Rendering.RefreshAllTileRenderers();
                IsBlendingEnabledChanged?.Invoke();
            }
        }

        public static void CalculateMasks(TileBlendSides blend, List<TileMaskSet> masks)
        {
            var blendWithLeft = (blend & Left) != 0;
            var blendWithUp = (blend & Up) != 0;
            var blendWithRight = (blend & Right) != 0;
            var blendWithDown = (blend & Down) != 0;
            var blendWithUpLeft = (blend & UpLeft) != 0;
            var blendWithUpRight = (blend & UpRight) != 0;
            var blendWithDownLeft = (blend & DownLeft) != 0;
            var blendWithDownRight = (blend & DownRight) != 0;

            if (blendWithLeft)
            {
                masks.Add(new TileMaskSet(TextureMaskVerticalArraySlice, isFlipHorizontally: true));
            }

            if (blendWithRight)
            {
                masks.Add(new TileMaskSet(TextureMaskVerticalArraySlice));
            }

            if (blendWithUp)
            {
                masks.Add(new TileMaskSet(TextureMaskHorizontalArraySlice));
            }

            if (blendWithDown)
            {
                masks.Add(new TileMaskSet(TextureMaskHorizontalArraySlice, isFlipVertically: true));
            }

            if (blendWithUpLeft
                && !blendWithUp
                && !blendWithLeft)
            {
                masks.Add(new TileMaskSet(TextureMaskCornerOuterArraySlice, isFlipVertically: true));
            }

            if (blendWithUpRight
                && !blendWithUp
                && !blendWithRight)
            {
                masks.Add(
                    new TileMaskSet(
                        TextureMaskCornerOuterArraySlice,
                        isFlipVertically: true,
                        isFlipHorizontally: true));
            }

            if (blendWithDownLeft
                && !blendWithDown
                && !blendWithLeft)
            {
                masks.Add(new TileMaskSet(TextureMaskCornerOuterArraySlice));
            }

            if (blendWithDownRight
                && !blendWithDown
                && !blendWithRight)
            {
                masks.Add(new TileMaskSet(TextureMaskCornerOuterArraySlice, isFlipHorizontally: true));
            }
        }

        public static RenderingMaterial CreateRenderingMaterial(
            TileBlendSides blendSides,
            TextureAtlasResource maskTexture)
        {
            var masks = new List<TileMaskSet>(capacity: 1);
            CalculateMasks(blendSides, masks);
            masks = DetectMasksInnerCorners(masks);
            Api.Assert(masks.Count > 0, "No masks?!");

            var effect = EffectForMaskCounts[masks.Count - 1];
            var material = RenderingMaterial.Create(effect);

            var effectParameters = material.EffectParameters;
            effectParameters.Set("MaskTextureArray", maskTexture);

            for (var i = 0; i < masks.Count; i++)
            {
                var mask = masks[i];
                var flip = (mask.IsFlipHorizontally ? 1f : 0f, mask.IsFlipVertically ? 1f : 0f);
                effectParameters.Set("Mask" + (i + 1) + "ArraySlice", mask.TextureMaskArraySlice)
                                .Set("Mask" + (i + 1) + "Flip", flip);
            }

            material.EffectResource = effect;
            return material;
        }

        public static void CreateTileBlendRenderers(
            Tile tile,
            ProtoTile tileProto,
            IClientSceneObject sceneObject)
        {
            if (!IsBlendingEnabled)
            {
                return;
            }

            var tileLeft = tile.NeighborTileLeft;
            var tileUp = tile.NeighborTileUp;
            var tileRight = tile.NeighborTileRight;
            var tileDown = tile.NeighborTileDown;
            var tileTopLeft = tile.GetNeighborTile(-1,    1);
            var tileTopRight = tile.GetNeighborTile(1,    1);
            var tileBottomLeft = tile.GetNeighborTile(-1, -1);
            var tileBottomRight = tile.GetNeighborTile(1, -1);

            CreateTileBlendRenderers(
                tile,
                tileProto,
                sceneObject,
                tileLeft,
                tileRight,
                tileUp,
                tileDown,
                tileTopLeft,
                tileTopRight,
                tileBottomLeft,
                tileBottomRight,
                isHeightsBlendPhase: false);

            CreateTileBlendRenderers(
                tile,
                tileProto,
                sceneObject,
                tileLeft,
                tileRight,
                tileUp,
                tileDown,
                tileTopLeft,
                tileTopRight,
                tileBottomLeft,
                tileBottomRight,
                isHeightsBlendPhase: true);
        }

        /// <summary>
        /// Checks for inner corners and generates new masks list.
        /// Inner corner is detected if horizontal and vertical masks pair found.
        /// </summary>
        public static List<TileMaskSet> DetectMasksInnerCorners(List<TileMaskSet> sourceMasks)
        {
            var result = new List<TileMaskSet>(capacity: sourceMasks.Count);
            var excudedIndexes = new HashSet<int>();

            for (var i = 0; i < sourceMasks.Count; i++)
            {
                var mask2 = sourceMasks[i];
                // please note - all masks were added sequentially - first all vertical, then all horizontal
                // so we will check check only vertical masks in this loop, and only horizontal masks in the sub-loop
                if (mask2.TextureMaskArraySlice != TextureMaskVerticalArraySlice)
                {
                    continue;
                }

                for (var j = i + 1; j < sourceMasks.Count; j++)
                {
                    var mask1 = sourceMasks[j];
                    if (mask1.TextureMaskArraySlice != TextureMaskHorizontalArraySlice)
                    {
                        continue;
                    }

                    // the inner corner found
                    // remember which masks we should not use anymore as they will be replaced by the inner corner mask
                    excudedIndexes.Add(i);
                    excudedIndexes.Add(j);
                    // add according inner corner mask
                    result.Add(
                        new TileMaskSet(
                            TextureMaskCornerInnerArraySlice,
                            isFlipHorizontally: mask2.IsFlipHorizontally,
                            isFlipVertically: mask1.IsFlipVertically));
                }
            }

            for (var index = 0; index < sourceMasks.Count; index++)
            {
                if (excudedIndexes.Contains(index))
                {
                    // we've replaced this vertical or horizontal mask with an inner corner mask
                    continue;
                }

                var mask = sourceMasks[index];
                result.Add(mask);
            }

            return result;
        }

        private static void CreateTileBlendRenderer(
            IClientSceneObject sceneObject,
            IRenderingClientService renderingService,
            ITextureResource blendWithTexture,
            RenderingMaterial blendRenderingMaterial)
        {
            var renderer = renderingService.CreateSpriteRenderer(
                sceneObject,
                blendWithTexture,
                drawOrder: DrawOrder.GroundBlend);
            renderer.RenderingMaterial = blendRenderingMaterial;
            renderer.SortByWorldPosition = false;
            renderer.IgnoreTextureQualityScaling = true;
            renderer.Size = ScriptingConstants.TileSizeRenderingVirtualSize;
        }

        private static void CreateTileBlendRenderers(
            Tile tile,
            ProtoTile tileProto,
            IClientSceneObject sceneObject,
            Tile tileLeft,
            Tile tileRight,
            Tile tileUp,
            Tile tileDown,
            Tile tileUpLeft,
            Tile tileUpRight,
            Tile tileDownLeft,
            Tile tileDownRight,
            bool isHeightsBlendPhase)
        {
            List<BlendLayer> blendLayers = null;

            var tileTexture = tileProto.GetGroundTexture(tile.Position);
            TryBlendWith(tile, tileTexture, isHeightsBlendPhase, tileLeft,      Left,      ref blendLayers);
            TryBlendWith(tile, tileTexture, isHeightsBlendPhase, tileRight,     Right,     ref blendLayers);
            TryBlendWith(tile, tileTexture, isHeightsBlendPhase, tileUp,        Up,        ref blendLayers);
            TryBlendWith(tile, tileTexture, isHeightsBlendPhase, tileDown,      Down,      ref blendLayers);
            TryBlendWith(tile, tileTexture, isHeightsBlendPhase, tileUpLeft,    UpLeft,    ref blendLayers);
            TryBlendWith(tile, tileTexture, isHeightsBlendPhase, tileUpRight,   UpRight,   ref blendLayers);
            TryBlendWith(tile, tileTexture, isHeightsBlendPhase, tileDownLeft,  DownLeft,  ref blendLayers);
            TryBlendWith(tile, tileTexture, isHeightsBlendPhase, tileDownRight, DownRight, ref blendLayers);

            if (blendLayers == null)
            {
                // no blending required
                return;
            }

            var renderingService = Api.Client.Rendering;
            foreach (var blendLayer in blendLayers)
            {
                var blendWith = blendLayer.BlendWith;
                var textureMask = isHeightsBlendPhase
                                      ? ProtoTile.TileCliffHeightEdgeMaskAtlas
                                      : blendWith.BlendMaskTexture;

                var blendRenderingMaterial = GetOrCreateCachedMaterial(blendLayer, textureMask);

                // create renderer for layer with mask
                CreateTileBlendRenderer(sceneObject, renderingService, blendWith.Texture, blendRenderingMaterial);

                if (!isHeightsBlendPhase
                    && tileProto.Kind == TileKind.Solid
                    && blendWith.ProtoTile.Kind == TileKind.Water)
                {
                    // create water blend
                    var protoTileWater = (IProtoTileWater)blendWith.ProtoTile;
                    ClientTileWaterHelper.CreateWaterRendererBlend(sceneObject, protoTileWater, blendLayer);
                }
            }
        }

        private static RenderingMaterial GetOrCreateCachedMaterial(
            BlendLayer blendLayer,
            TextureAtlasResource maskTexture)
        {
            var cacheKey = new MaterialCacheKey(blendLayer.BlendSides, maskTexture);
            if (CachedBlendMaskMaterials.TryGetValue(cacheKey, out var material))
            {
                return material;
            }

            material = CreateRenderingMaterial(blendLayer.BlendSides, maskTexture);
            CachedBlendMaskMaterials[cacheKey] = material;
            return material;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void TryBlendWith(
            Tile currentTile,
            ProtoTileGroundTexture currentTileTexture,
            bool isHeightsBlendPhase,
            Tile otherTile,
            TileBlendSides appendBlendMask,
            ref List<BlendLayer> blendLayers)
        {
            if (otherTile.IsOutOfBounds)
            {
                return;
            }

            var otherTileTexture = ((ProtoTile)otherTile.ProtoTile).GetGroundTexture(otherTile.Position);
            if (isHeightsBlendPhase)
            {
                if (currentTile.Height >= otherTile.Height
                    || currentTile.IsSlope && otherTile.IsSlope)
                {
                    // no need to blend because same tiles height (or other tile higher)
                    // or both tiles are slopes
                    return;
                }

                // please note - will blend even if the same ground tile type
                // because height is different and ground on lower layer can be blended with neighbor tiles
            }
            else // if not a heights blend phase
            {
                if (currentTileTexture == otherTileTexture)
                {
                    // no need to blend because ground type is the same
                    return;
                }

                if (currentTile.Height != otherTile.Height)
                {
                    // no need to blend because different height levels
                    return;
                }
            }

            if (!isHeightsBlendPhase
                && currentTileTexture.CalculatedBlendOrder
                > otherTileTexture.CalculatedBlendOrder)
            {
                // no need to blend because other tile blend order is higher
                return;
            }

            if (blendLayers != null)
            {
                for (var index = 0; index < blendLayers.Count; index++)
                {
                    var blendLayer = blendLayers[index];
                    if (blendLayer.BlendWith != otherTileTexture)
                    {
                        continue;
                    }

                    // found layer of the same type - append mask
                    blendLayer.BlendSides |= appendBlendMask;
                    blendLayers[index] = blendLayer;
                    return;
                }
            }
            else
            {
                blendLayers = new List<BlendLayer>();
            }

            // no layers of the same type exists - create new layer and insert it into the list
            var newBlendLayer = new BlendLayer(appendBlendMask, otherTileTexture);
            var newBlendLayerBlendOrder = otherTileTexture.CalculatedBlendOrder;

            for (var index = 0; index < blendLayers.Count; index++)
            {
                var otherBlendLayer = blendLayers[index];
                var otherBlendLayerBlendOrder = otherBlendLayer.BlendWith.CalculatedBlendOrder;
                if (otherBlendLayerBlendOrder < newBlendLayerBlendOrder)
                {
                    // blend layer is higher than current
                    continue;
                }

                if (otherBlendLayerBlendOrder == newBlendLayerBlendOrder)
                {
                    // error case, do not blend these layers!
                    continue;
                }

                // new blend layer has a lower order - it should go before this element
                blendLayers.Insert(index, newBlendLayer);
                return;
            }

            blendLayers.Add(newBlendLayer);
        }

        public struct BlendLayer
        {
            public readonly ProtoTileGroundTexture BlendWith;

            public TileBlendSides BlendSides;

            public BlendLayer(TileBlendSides blendSides, ProtoTileGroundTexture blendWith)
            {
                this.BlendSides = blendSides;
                this.BlendWith = blendWith;
            }
        }

        private struct MaterialCacheKey : IEquatable<MaterialCacheKey>
        {
            public readonly TileBlendSides BlendSides;

            public readonly TextureAtlasResource MaskTextureAtlasResource;

            public MaterialCacheKey(TileBlendSides blendSides, TextureAtlasResource maskTextureAtlasResource)
            {
                this.BlendSides = blendSides;
                this.MaskTextureAtlasResource = maskTextureAtlasResource;
            }

            public bool Equals(MaterialCacheKey other)
            {
                return this.BlendSides == other.BlendSides
                       && this.MaskTextureAtlasResource
                              .Equals(other.MaskTextureAtlasResource);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is MaterialCacheKey && this.Equals((MaterialCacheKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((int)this.BlendSides * 397)
                           ^ (this.MaskTextureAtlasResource?.GetHashCode() ?? 0);
                }
            }
        }
    }
}