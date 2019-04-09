namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    internal static class ClientTileCliffsRendererHelper
    {
        private static readonly IRenderingClientService RenderingService = Api.Client.Rendering;

        public static void CreateCliffRenderer(
            IClientSceneObject sceneObject,
            ColumnRow textureRegion,
            TextureAtlasResource cliffAtlas)
        {
            RenderingService.CreateSpriteRenderer(
                sceneObject,
                cliffAtlas.Chunk(textureRegion.Column, textureRegion.Row),
                drawOrder: DrawOrder.GroundCliffs,
                positionOffset: (0, 0));
        }

        public static void CreateCliffsRenderersIfNeeded(
            Tile tile,
            IClientSceneObject sceneObject,
            TextureAtlasResource cliffAtlas)
        {
            if (!tile.IsSlope
                && !tile.IsCliff)
            {
                return;
            }

            if (tile.IsSlope)
            {
                var isSlopeUp = tile.NeighborTileUp.Height > tile.Height;
                if (isSlopeUp)
                {
                    if (!tile.NeighborTileRight.IsSlope)
                    {
                        CreateCliffRenderer(sceneObject, CliffTextureRegion.SlopeBottomRight, cliffAtlas);
                    }

                    if (!tile.NeighborTileLeft.IsSlope)
                    {
                        CreateCliffRenderer(sceneObject, CliffTextureRegion.SlopeBottomLeft, cliffAtlas);
                    }
                }
                else // if slope down
                {
                    if (!tile.NeighborTileRight.IsSlope)
                    {
                        CreateCliffRenderer(sceneObject, CliffTextureRegion.SlopeTopRight, cliffAtlas);
                    }

                    if (!tile.NeighborTileLeft.IsSlope)
                    {
                        CreateCliffRenderer(sceneObject, CliffTextureRegion.SlopeTopLeft, cliffAtlas);
                    }
                }

                return;
            }

            var height = tile.Height;

            // create cliffs rendering if needed
            var left = IsHigherTile(tile,  height, -1, 0);
            var up = IsHigherTile(tile,    height, 0,  1);
            var right = IsHigherTile(tile, height, 1,  0);
            var down = IsHigherTile(tile,  height, 0,  -1);

            if (left
                && up
                && right
                && down)
            {
                CreateCliffRenderer(sceneObject, CliffTextureRegion.FourSidesInnerCorner, cliffAtlas);
                return;
            }

            var upLeft = IsHigherTile(tile,    height, -1, 1);
            var upRight = IsHigherTile(tile,   height, 1,  1);
            var downLeft = IsHigherTile(tile,  height, -1, -1);
            var downRight = IsHigherTile(tile, height, 1,  -1);

            if (down)
            {
                // down tile is higher height
                if (!left
                    && !right)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.TopCenter, cliffAtlas);
                }
                else if (left && right)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.TopLeftRightInnerCorner, cliffAtlas);
                }
                else if (right && !up)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.TopLeftInnerCorner, cliffAtlas);
                }
                else if (left && !up)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.TopRightInnerCorner, cliffAtlas);
                }
            }

            if (up)
            {
                if (!left
                    && !right)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.BottomCenter, cliffAtlas);
                }
                else if (left && right)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.BottomLeftRightInnerCorner, cliffAtlas);
                }
                else if (right && !down)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.BottomLeftInnerCorner, cliffAtlas);
                }
                else if (left && !down)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.BottomRightInnerCorner, cliffAtlas);
                }
            }

            if (up && down)
            {
                if (left)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.TopBottomRightInnerCorner, cliffAtlas);
                }
                else if (right)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.TopBottomLeftInnerCorner, cliffAtlas);
                }
            }

            if (!down)
            {
                if (left && !up)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.MiddleTiledRight, cliffAtlas);
                }

                if (right && !up)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.MiddleTiledLeft, cliffAtlas);
                }

                if (downLeft
                    && downRight
                    && !left
                    && !right)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.TopRightCorner, cliffAtlas);
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.TopLeftCorner,  cliffAtlas);
                }
                else if (downLeft && !left)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.TopRightCorner, cliffAtlas);
                }
                else if (downRight && !right)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.TopLeftCorner, cliffAtlas);
                }
            }

            if (!up)
            {
                if (upLeft
                    && upRight
                    && !left
                    && !right)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.BottomRight, cliffAtlas);
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.BottomLeft,  cliffAtlas);
                }
                else if (upLeft && !left)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.BottomRight, cliffAtlas);
                }
                else if (upRight && !right)
                {
                    CreateCliffRenderer(sceneObject, CliffTextureRegion.BottomLeft, cliffAtlas);
                }
            }
        }

        private static bool IsHigherTile(Tile sourceTile, byte height, int offsetX, int offsetY)
        {
            var neighborTile = sourceTile.GetNeighborTile(offsetX, offsetY);
            return neighborTile.Height > height;
        }
    }
}