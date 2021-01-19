namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// Bucket-style spatial cache for the land claims to make accessing them much more faster.
    /// </summary>
    public class LandClaimAreasCache
    {
        public static readonly int MaxSize
            = 3 * Math.Max((int)LandClaimSystem.MaxLandClaimSizeWithGraceArea.Value, 20);

        private readonly Dictionary<Vector2Ushort, List<ILogicObject>> areasBySector
            = new();

        public LandClaimAreasCache(IList<ILogicObject> landClaimAreas)
        {
            foreach (var area in landClaimAreas)
            {
                var areaBounds = LandClaimSystem.SharedGetLandClaimAreaBounds(area, addGracePadding: true);

                AddArea(area, (areaBounds.Left, areaBounds.Top));
                AddArea(area, (areaBounds.Left, areaBounds.Bottom));
                AddArea(area, (areaBounds.Right, areaBounds.Top));
                AddArea(area, (areaBounds.Right, areaBounds.Bottom));
                AddArea(area, (areaBounds.Left + areaBounds.Width / 2, areaBounds.Bottom + areaBounds.Height / 2));

                void AddArea(ILogicObject area, Vector2Int position)
                {
                    var sectorPosition = CalculateSectorPosition(position);
                    if (!this.areasBySector.TryGetValue(sectorPosition, out var list))
                    {
                        list = new List<ILogicObject>(capacity: 3);
                        this.areasBySector[sectorPosition] = list;
                    }

                    list.AddIfNotContains(area);
                }
            }
        }

        public IEnumerable<ILogicObject> EnumerateForBounds(RectangleInt bounds)
        {
            RectangleInt sectorBounds;
            {
                var bx1 = bounds.X / (double)MaxSize;
                var by1 = bounds.Y / (double)MaxSize;
                var bx2 = (bounds.X + bounds.Width) / (double)MaxSize;
                var by2 = (bounds.Y + bounds.Height) / (double)MaxSize;

                var bxRounded = (ushort)Math.Max(0, bx1);
                var byRounded = (ushort)Math.Max(0, by1);
                sectorBounds = new RectangleInt(
                    x: bxRounded,
                    y: byRounded,
                    width: (ushort)Math.Ceiling(Math.Max(1,  bx2 - bxRounded)),
                    height: (ushort)Math.Ceiling(Math.Max(1, by2 - byRounded)));
            }

            for (ushort x = 0; x < sectorBounds.Width; x++)
            for (ushort y = 0; y < sectorBounds.Height; y++)
            {
                var sectorPosition = new Vector2Ushort((ushort)(sectorBounds.X + x),
                                                       (ushort)(sectorBounds.Y + y));
                if (!this.areasBySector.TryGetValue(sectorPosition, out var sectorAreas))
                {
                    continue;
                }

                foreach (var area in sectorAreas)
                {
                    yield return area;
                }
            }
        }

        public IReadOnlyList<ILogicObject> EnumerateForPosition(in Vector2Ushort tilePosition)
        {
            var sectorPosition = CalculateSectorPosition(tilePosition);
            return this.areasBySector.TryGetValue(sectorPosition, out var result)
                       ? (IReadOnlyList<ILogicObject>)result
                       : Array.Empty<ILogicObject>();
        }

        private static Vector2Ushort CalculateSectorPosition(in Vector2Int position)
        {
            return new((ushort)Math.Max(0, position.X / MaxSize),
                       (ushort)Math.Max(0, position.Y / MaxSize));
        }
    }
}