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
            IClientSceneObject sceneObject)
        {
            if (!tile.IsSlope
                && !tile.IsCliff)
            {
                return;
            }

            var neighborTileDown = tile.NeighborTileDown;
            var neighborTileUp = tile.NeighborTileUp;

            if (tile.IsSlope)
            {
                var isSlopeUp = neighborTileUp.Height > tile.Height;
                if (isSlopeUp)
                {
                    if (!tile.NeighborTileRight.IsSlope)
                    {
                        CreateCliffRenderer(sceneObject,
                                            CliffTextureRegion.SlopeBottomRight,
                                            GetCliffAtlas(neighborTileUp));
                    }

                    if (!tile.NeighborTileLeft.IsSlope)
                    {
                        CreateCliffRenderer(sceneObject,
                                            CliffTextureRegion.SlopeBottomLeft,
                                            GetCliffAtlas(neighborTileUp));
                    }
                }
                else // if slope down
                {
                    if (!tile.NeighborTileRight.IsSlope)
                    {
                        CreateCliffRenderer(sceneObject,
                                            CliffTextureRegion.SlopeTopRight,
                                            GetCliffAtlas(neighborTileDown));
                    }

                    if (!tile.NeighborTileLeft.IsSlope)
                    {
                        CreateCliffRenderer(sceneObject,
                                            CliffTextureRegion.SlopeTopLeft,
                                            GetCliffAtlas(neighborTileDown));
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
                CreateCliffRenderer(sceneObject,
                                    CliffTextureRegion.FourSidesInnerCorner,
                                    GetCliffAtlas(neighborTileDown));
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
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.TopCenter,
                                        GetCliffAtlas(neighborTileDown));
                }
                else if (left && right)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.TopLeftRightInnerCorner,
                                        GetCliffAtlas(neighborTileDown));
                }
                else if (right && !up)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.TopLeftInnerCorner,
                                        GetCliffAtlas(neighborTileDown));
                }
                else if (left && !up)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.TopRightInnerCorner,
                                        GetCliffAtlas(neighborTileDown));
                }
            }

            if (up)
            {
                if (!left
                    && !right)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.BottomCenter,
                                        GetCliffAtlas(neighborTileUp));
                }
                else if (left && right)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.BottomLeftRightInnerCorner,
                                        GetCliffAtlas(neighborTileUp));
                }
                else if (right && !down)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.BottomLeftInnerCorner,
                                        GetCliffAtlas(neighborTileUp));
                }
                else if (left && !down)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.BottomRightInnerCorner,
                                        GetCliffAtlas(neighborTileUp));
                }
            }

            if (up && down)
            {
                if (left)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.TopBottomRightInnerCorner,
                                        GetCliffAtlas(neighborTileDown));
                }
                else if (right)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.TopBottomLeftInnerCorner,
                                        GetCliffAtlas(neighborTileDown));
                }
            }

            if (!down)
            {
                if (left && !up)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.MiddleTiledRight,
                                        GetCliffAtlas(tile.NeighborTileLeft));
                }

                if (right && !up)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.MiddleTiledLeft,
                                        GetCliffAtlas(tile.NeighborTileRight));
                }

                if (downLeft
                    && downRight
                    && !left
                    && !right)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.TopRightCorner,
                                        GetCliffAtlas(tile.NeighborTileUpLeft));
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.TopLeftCorner,
                                        GetCliffAtlas(tile.NeighborTileUpRight));
                }
                else if (downLeft && !left)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.TopRightCorner,
                                        GetCliffAtlas(tile.NeighborTileDownLeft));
                }
                else if (downRight && !right)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.TopLeftCorner,
                                        GetCliffAtlas(tile.NeighborTileDownRight));
                }
            }

            if (!up)
            {
                if (upLeft
                    && upRight
                    && !left
                    && !right)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.BottomRight,
                                        GetCliffAtlas(tile.NeighborTileUpLeft));
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.BottomLeft,
                                        GetCliffAtlas(tile.NeighborTileUpRight));
                }
                else if (upLeft && !left)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.BottomRight,
                                        GetCliffAtlas(tile.NeighborTileUpLeft));
                }
                else if (upRight && !right)
                {
                    CreateCliffRenderer(sceneObject,
                                        CliffTextureRegion.BottomLeft,
                                        GetCliffAtlas(tile.NeighborTileUpRight));
                }
            }
        }

        private static TextureAtlasResource GetCliffAtlas(Tile tile)
            => ((ProtoTile)tile.ProtoTile).CliffAtlas;

        private static bool IsHigherTile(Tile sourceTile, byte height, int offsetX, int offsetY)
        {
            var neighborTile = sourceTile.GetNeighborTile(offsetX, offsetY);
            return neighborTile.Height > height;
        }
    }
}