namespace AtomicTorch.CBND.CoreMod.Zones.Spawn
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.GameApi.Data.Structures;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ZoneChunksHelper
    {
        public static async Task<Dictionary<Vector2Ushort, ZoneChunkFilledCellsCounter>> CalculateZoneChunks(
            IQuadTreeNode quadTree,
            int chunkSize,
            Func<Task> callbackYieldIfOutOfTime)
        {
            // this is a heavy method so we will try to yield every few nodes to reduce the load
            const int defaultCounterToYieldValue = 100;
            var counterToYield = defaultCounterToYieldValue;

            var dict = new Dictionary<Vector2Ushort, ZoneChunkFilledCellsCounter>();

            if (false)
            {
                // slow algorithm (each tile added separately into the dictionary)
                ZoneChunkFilledCellsCounter lastCounter = new ZoneChunkFilledCellsCounter(Vector2Ushort.Max);
                foreach (var filledNode in quadTree.EnumerateFilledNodes())
                {
                    var size = filledNode.Size;
                    var startPosition = filledNode.Position;
                    var endPosition = startPosition + new Vector2Ushort(size, size);
                    for (var y = startPosition.Y; y < endPosition.Y; y++)
                    for (var x = startPosition.X; x < endPosition.X; x++)
                    {
                        var chunkPosition = new Vector2Ushort((ushort)(x / chunkSize), (ushort)(y / chunkSize));
                        if (lastCounter.ChunkOffset == chunkPosition
                            || dict.TryGetValue(chunkPosition, out lastCounter))
                        {
                            lastCounter.Count++;
                        }
                        else
                        {
                            lastCounter = new ZoneChunkFilledCellsCounter(chunkPosition) { Count = 1 };
                            dict[chunkPosition] = lastCounter;
                        }
                    }
                }
            }

            // fast algorithm (each quad tree node can instantly fill each chunk)
            foreach (var node in quadTree.EnumerateFilledNodes())
            {
                await YieldIfOutOfTime();

                var nodeStartPosition = node.Position;
                var startChunksOffset = new Vector2Ushort(
                    (ushort)(chunkSize * (nodeStartPosition.X / chunkSize)),
                    (ushort)(chunkSize * (nodeStartPosition.Y / chunkSize)));

                var nodeSize = node.Size;
                if (nodeSize == 1)
                {
                    // short path
                    var chunkPosition = new Vector2Ushort(startChunksOffset.X, startChunksOffset.Y);
                    if (!dict.TryGetValue(chunkPosition, out var counter))
                    {
                        counter = new ZoneChunkFilledCellsCounter(chunkPosition);
                        dict[chunkPosition] = counter;
                    }

                    counter.Count++;
                    continue;
                }

                // long path (for nodes > 1*1 size)
                var nodeEndPosition = nodeStartPosition.AddAndClamp(new Vector2Ushort(node.Size, node.Size));

                var endChunksOffset = new Vector2Ushort(
                    (ushort)(chunkSize * Math.Ceiling(nodeEndPosition.X / (double)chunkSize)),
                    (ushort)(chunkSize * Math.Ceiling(nodeEndPosition.Y / (double)chunkSize)));

                for (var chunkY = startChunksOffset.Y;
                     chunkY < endChunksOffset.Y;
                     chunkY = (ushort)(chunkY + chunkSize))
                for (var chunkX = startChunksOffset.X;
                     chunkX < endChunksOffset.X;
                     chunkX = (ushort)(chunkX + chunkSize))
                {
                    var chunkPosition = new Vector2Ushort(chunkX, chunkY);
                    var cellsCount = CalculateCellsCount(nodeStartPosition, nodeEndPosition, chunkX, chunkY, chunkSize);
                    if (!dict.TryGetValue(chunkPosition, out var counter))
                    {
                        counter = new ZoneChunkFilledCellsCounter(chunkPosition);
                        dict[chunkPosition] = counter;
                    }

                    counter.Count += cellsCount;
                }
            }

            return dict;

            Task YieldIfOutOfTime()
            {
                if (--counterToYield > 0)
                {
                    return Task.CompletedTask;
                }

                counterToYield = defaultCounterToYieldValue;
                return callbackYieldIfOutOfTime();
            }
        }

        private static int CalculateCellsCount(
            Vector2Ushort nodeStartPosition,
            Vector2Ushort nodeEndPosition,
            ushort chunkStartX,
            ushort chunkStartY,
            int chunkSize)
        {
            ushort chunkEndX = (ushort)(chunkStartX + chunkSize),
                   chunkEndY = (ushort)(chunkStartY + chunkSize);

            // clamp chunk size inside the node bounds
            if (chunkStartX < nodeStartPosition.X)
            {
                chunkStartX = nodeStartPosition.X;
            }

            if (chunkStartY < nodeStartPosition.Y)
            {
                chunkStartY = nodeStartPosition.Y;
            }

            if (chunkEndX > nodeEndPosition.X)
            {
                chunkEndX = nodeEndPosition.X;
            }

            if (chunkEndY > nodeEndPosition.Y)
            {
                chunkEndY = nodeEndPosition.Y;
            }

            var sizeX = chunkEndX - chunkStartX;
            var sizeY = chunkEndY - chunkStartY;
            return sizeX * sizeY;
        }

        public class ZoneChunkFilledCellsCounter
        {
            public Vector2Ushort ChunkOffset;

            public int Count;

            public ZoneChunkFilledCellsCounter(Vector2Ushort chunkOffset)
            {
                this.ChunkOffset = chunkOffset;
            }
        }
    }
}