namespace AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class EditorTileHelper
    {
        public static byte CalculateAverageHeight(List<Vector2Ushort> tilePositions)
        {
            var world = Api.Client.World;
            var totalHeight = 0d;
            foreach (var tilePosition in tilePositions)
            {
                totalHeight += world.GetTile(tilePosition).Height;
            }

            return (byte)Math.Round(totalHeight / tilePositions.Count, MidpointRounding.AwayFromZero);
        }

        public static ProtoTile CalculateMostFrequentTileProto(List<Vector2Ushort> tilePositions)
        {
            var world = Api.Client.World;
            var protoCount = new Dictionary<IProtoTile, int>();
            foreach (var tilePosition in tilePositions)
            {
                var protoTile = world.GetTile(tilePosition).ProtoTile;
                protoCount.TryGetValue(protoTile, out var currentValue);
                protoCount[protoTile] = currentValue + 1;
            }

            var orderedByFrequency = protoCount.OrderByDescending(p => p.Value);
            return (ProtoTile)orderedByFrequency.FirstOrDefault().Key;
        }

        /// <summary>
        /// Recursively gathers all the tiles around of the same proto tile as the tile at the starting position.
        /// </summary>
        public static List<Vector2Ushort> GatherAllTilePositionsOfTheSameProtoTile(
            Vector2Ushort startTilePosition,
            bool onlyOnTheSameHeight,
            bool ignoreCliffsAndSlopes)
        {
            var world = Api.Client.World;
            var startTile = world.GetTile(startTilePosition);
            var requiredProtoTile = startTile.ProtoTile;
            var requiredHeight = startTile.Height;

            var tilesToCheck = new Stack<Tile>(capacity: 100);
            tilesToCheck.Push(startTile);

            var result = new HashSet<Vector2Ushort>() { startTilePosition };

            while (tilesToCheck.Count > 0)
            {
                var tile = tilesToCheck.Pop();
                if (tile.ProtoTile != requiredProtoTile)
                {
                    continue;
                }

                if (onlyOnTheSameHeight
                    && tile.Height != requiredHeight)
                {
                    continue;
                }

                if (ignoreCliffsAndSlopes
                    && tile.IsCliffOrSlope)
                {
                    continue;
                }

                result.Add(tile.Position);
                foreach (var neighborTile in tile.EightNeighborTiles)
                {
                    if (!result.Contains(neighborTile.Position))
                    {
                        tilesToCheck.Push(neighborTile);
                    }
                }
            }

            return result.ToList();
        }
    }
}