namespace AtomicTorch.CBND.CoreMod.Tiles
{
    using System.Collections.Generic;

    /// <summary>
    /// To avoid blend orders collision, we could also perform sort-ordering.
    /// The proto tiles are registered one-by-one in determined order.
    /// So if there is an already a registered blend order,
    /// the following proto tiles with the same blend order will have some blend order offset
    /// (accordingly to their registration order).
    /// </summary>
    internal static class ProtoTileBlendOrdersCalculator
    {
        private static readonly List<(byte blendOrder, int texturesCount)> BlendOrdering
            = new();

        public static int CalculateBlendOrder(ProtoTile protoTile)
        {
            var order = protoTile.BlendOrder;

            var offset = 0;
            for (var index = 0; index < BlendOrdering.Count; index++)
            {
                var pair = BlendOrdering[index];
                if (pair.blendOrder != order)
                {
                    continue;
                }

                // found blend order
                pair.texturesCount++;
                BlendOrdering[index] = pair;
                offset = pair.texturesCount;
                break;
            }

            if (offset == 0)
            {
                // not found blend order
                offset = 1;
                BlendOrdering.Add((order, offset));
            }

            return order * 255 + offset;
        }
    }
}