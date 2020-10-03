namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientConstructionSiteOutlineHelper
    {
        // construction outline atlas
        private static readonly ITextureAtlasResource AtlasTextureResource
            = new TextureAtlasResource(
                "StaticObjects/Structures/ConstructionSite/ObjectConstructionSiteAtlas.png",
                columns: 4,
                rows: 4,
                isTransparent: true);

        private static readonly IRenderingClientService Rendering = Api.Client.Rendering;

        private static readonly Vector2D SpritesOffset = (0, -0.05);

        public static void CreateOutlineRenderer(IStaticWorldObject worldObject, IProtoObjectStructure protoStructure)
        {
            foreach (var tileOffset in protoStructure.Layout.TileOffsets)
            {
                var (front, side, back) = GetSprite(worldObject.OccupiedTile, tileOffset, protoStructure);
                if (side is not null)
                {
                    Rendering.CreateSpriteRenderer(
                                 worldObject,
                                 side,
                                 positionOffset: tileOffset.ToVector2D() + SpritesOffset)
                             .DrawOrderOffsetY = 1;
                }

                if (back is not null)
                {
                    Rendering.CreateSpriteRenderer(
                                 worldObject,
                                 back,
                                 positionOffset: tileOffset.ToVector2D() + SpritesOffset)
                             .DrawOrderOffsetY = 1;
                }

                if (front is not null)
                {
                    Rendering.CreateSpriteRenderer(
                        worldObject,
                        front,
                        positionOffset: tileOffset.ToVector2D() + SpritesOffset);
                }
            }
        }

        private static Vector2Int GetNeighborTileOffset(Vector2Int tileOffset, int index, out NeighborsPattern pattern)
        {
            switch (index)
            {
                case 0:
                    pattern = NeighborsPattern.Left;
                    return new Vector2Int(tileOffset.X - 1, tileOffset.Y);
                case 1:
                    pattern = NeighborsPattern.Top;
                    return new Vector2Int(tileOffset.X, tileOffset.Y + 1);
                case 2:
                    pattern = NeighborsPattern.Right;
                    return new Vector2Int(tileOffset.X + 1, tileOffset.Y);
                case 3:
                    pattern = NeighborsPattern.Bottom;
                    return new Vector2Int(tileOffset.X, tileOffset.Y - 1);
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        private static (ITextureResource front, ITextureResource side, ITextureResource back) GetSprite(
            Tile tile,
            Vector2Int tileOffset,
            IProtoObjectStructure protoStructure)
        {
            var layout = protoStructure.Layout;
            var neighbors = NeighborsPattern.None;

            for (var i = 0; i < 4; i++)
            {
                var neighborTileOffset = GetNeighborTileOffset(tileOffset, i, out var pattern);
                if (layout.TileOffsets.Contains(neighborTileOffset))
                {
                    // this proto structure contains tile in its layout
                    neighbors |= pattern;
                }

                ////// commented out - because we don't have inner corner sprites in outline texture atlas
                //// join outline of neighbor blueprint objects of the same type
                //else
                //{
                //    // check if there is a joinable neighbor nearby
                //    var neighborTile = tile.GetNeighborTile(neighborTileOffset);
                //    if (IsOutlineJoinable(neighborTile, protoStructure))
                //    {
                //        neighbors |= pattern;
                //    }
                //}
            }

            var chunkNullable = ConstructionSiteOutlinePatterns.GetChunk(neighbors);
            if (!chunkNullable.HasValue)
            {
                return (null, null, null);
            }

            var chunk = chunkNullable.Value;
            ITextureResource front = null, side = null, back = null;
            if (chunk.FrontColumn.HasValue)
            {
                front = AtlasTextureResource.Chunk(chunk.FrontColumn.Value, chunk.FrontRow.Value);
            }

            if (chunk.SideColumn.HasValue)
            {
                side = AtlasTextureResource.Chunk(chunk.SideColumn.Value, chunk.SideRow.Value);
            }

            if (chunk.BackColumn.HasValue)
            {
                back = AtlasTextureResource.Chunk(chunk.BackColumn.Value, chunk.BackRow.Value);
            }

            return (front, side, back);
        }

        private static bool IsOutlineJoinable(Tile tile, IProtoObjectStructure protoStructure)
        {
            return tile.StaticObjects.Any(
                o => !o.IsDestroyed
                     && ProtoObjectConstructionSite.SharedIsConstructionOf(o, protoStructure));
        }
    }
}