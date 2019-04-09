namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;
    using static Tiles.TileBlendSides;

    public class ClientFarmPlotBlendHelper
    {
        private readonly Dictionary<TileBlendSides, RenderingMaterial> cachedBlendMaskMaterials
            = new Dictionary<TileBlendSides, RenderingMaterial>();

        private readonly TextureAtlasResource maskTexture;

        private readonly IProtoObjectFarmPlot protoFarmPlot;

        private readonly string sceneObjectsName;

        private readonly Dictionary<Vector2Ushort, TileRenderingData> tileDictionary
            = new Dictionary<Vector2Ushort, TileRenderingData>();

        public ClientFarmPlotBlendHelper(IProtoObjectFarmPlot protoFarmPlot)
        {
            this.protoFarmPlot = protoFarmPlot;
            this.maskTexture = this.protoFarmPlot.BlendMaskTextureAtlas;
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            this.sceneObjectsName = this.protoFarmPlot.Name + "BlendRenderer";

            Api.Client.World.WorldBoundsChanged += this.WorldBoundsChangedHandler;
        }

        public void Update(Tile tile)
        {
            var frameNumber = Api.Client.CurrentGame.ServerFrameNumber;
            this.RefreshRendering(tile, frameNumber);

            foreach (var neighborTile in tile.EightNeighborTiles)
            {
                this.RefreshRendering(neighborTile, frameNumber);
            }
        }

        private BlendLayers GetBlendLayers(Tile tile)
        {
            if (this.HasFarmPlot(tile))
            {
                // don't blend such object - tile contains farm
                return BlendLayers.Empty;
            }

            var blendLayers = new BlendLayers();
            var tileLeft = tile.NeighborTileLeft;
            var tileUp = tile.NeighborTileUp;
            var tileRight = tile.NeighborTileRight;
            var tileDown = tile.NeighborTileDown;
            var tileUpLeft = tile.GetNeighborTile(-1,   1);
            var tileUpRight = tile.GetNeighborTile(1,   1);
            var tileDownLeft = tile.GetNeighborTile(-1, -1);
            var tileDownRight = tile.GetNeighborTile(1, -1);

            var blendSides = None;

            void TryBlendWith(Tile targetTile, TileBlendSides side)
            {
                if (this.HasFarmPlot(targetTile))
                {
                    blendSides |= side;
                }
            }

            TryBlendWith(tileLeft,      Left);
            TryBlendWith(tileUp,        Up);
            TryBlendWith(tileRight,     Right);
            TryBlendWith(tileDown,      Down);
            TryBlendWith(tileUpLeft,    UpLeft);
            TryBlendWith(tileUpRight,   UpRight);
            TryBlendWith(tileDownLeft,  DownLeft);
            TryBlendWith(tileDownRight, DownRight);

            blendLayers.TileBlendSides = blendSides;

            return blendLayers;
        }

        private RenderingMaterial GetBlendMaterial(
            BlendLayers blendLayer)
        {
            var cacheKey = blendLayer.TileBlendSides;
            if (cacheKey == None)
            {
                throw new Exception();
            }

            if (this.cachedBlendMaskMaterials.TryGetValue(cacheKey, out var material))
            {
                return material;
            }

            material = ClientTileBlendHelper.CreateRenderingMaterial(cacheKey, this.maskTexture);
            this.cachedBlendMaskMaterials[cacheKey] = material;
            return material;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasFarmPlot(Tile tile)
        {
            foreach (var s in tile.StaticObjects)
            {
                if (!s.IsDestroyed
                    && s.ProtoStaticWorldObject == this.protoFarmPlot)
                {
                    return true;
                }
            }

            return false;
        }

        private void RefreshRendering(Tile tile, uint frameNumber)
        {
            if (!this.tileDictionary.TryGetValue(tile.Position, out var data))
            {
                // try register tile
                data = new TileRenderingData(frameNumber);
                this.SetupTileRendering(data, tile);

                if (data.SpriteRenderer != null
                    && data.SpriteRenderer.IsEnabled)
                {
                    this.tileDictionary[tile.Position] = data;
                }

                return;
            }

            // COMMENTED OUT - because we cannot use this optimization yet
            // there are multiple objects could be added or deleted at single frame.
            //if (data.LastUpdateFrameNumber == frameNumber)
            //{
            //    // no need to refresh
            //    return;
            //}

            data.LastUpdateFrameNumber = frameNumber;
            this.SetupTileRendering(data, tile);

            if (data.SpriteRenderer == null
                || !data.SpriteRenderer.IsEnabled)
            {
                this.tileDictionary.Remove(tile.Position);
            }
        }

        private void SetupTileRendering(TileRenderingData data, Tile tile)
        {
            var renderer = data.SpriteRenderer;
            var blendLayers = this.GetBlendLayers(tile);
            if (blendLayers.TileBlendSides == None)
            {
                // tile rendering not required
                if (renderer != null)
                {
                    renderer.IsEnabled = false;
                }

                return;
            }

            if (renderer == null)
            {
                renderer = Api.Client.Rendering.CreateSpriteRenderer(
                    Api.Client.Scene.CreateSceneObject(
                        this.sceneObjectsName,
                        position: tile.Position.ToVector2D()),
                    this.protoFarmPlot.Texture,
                    drawOrder: DrawOrder.GroundBlend + 3);
                renderer.SortByWorldPosition = false;
                renderer.IgnoreTextureQualityScaling = true;
                renderer.Size = ScriptingConstants.TileSizeRenderingVirtualSize;

                data.SpriteRenderer = renderer;
            }

            renderer.IsEnabled = true;
            renderer.RenderingMaterial = this.GetBlendMaterial(blendLayers);
        }

        private void WorldBoundsChangedHandler()
        {
            foreach (var data in this.tileDictionary.Values)
            {
                data.SpriteRenderer?.SceneObject.Destroy();
            }

            this.tileDictionary.Clear();
        }

        internal class BlendLayers
        {
            public static readonly BlendLayers Empty = new BlendLayers();

            public TileBlendSides TileBlendSides;
        }

        private class TileRenderingData
        {
            public ulong LastUpdateFrameNumber;

            public IComponentSpriteRenderer SpriteRenderer;

            public TileRenderingData(ulong lastUpdateFrameNumber)
            {
                this.LastUpdateFrameNumber = lastUpdateFrameNumber;
            }
        }
    }
}