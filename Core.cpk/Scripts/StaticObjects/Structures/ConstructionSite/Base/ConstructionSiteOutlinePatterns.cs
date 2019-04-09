namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using static Helpers.Primitives.NeighborsPattern;

    public static class ConstructionSiteOutlinePatterns
    {
        // ReSharper disable once InconsistentNaming
        private static readonly byte? no = null;

        private static readonly Dictionary<NeighborsPattern, Chunks> PatternToChunk;

        static ConstructionSiteOutlinePatterns()
        {
            PatternToChunk = new Dictionary<NeighborsPattern, Chunks>
            {
                // { neighbors pattern, atlas chunks (front column+row, side c+r, back c+r)}
                // no neighbors
                { None, new Chunks(3, 2, 0, 1, 3, 3) },
                // one side neighbors
                { Left, new Chunks(2,    2,  2, 1, 2,  3) },
                { Top, new Chunks(3,     2,  0, 1, no, no) },
                { Right, new Chunks(0,   2,  1, 1, 0,  3) },
                { Bottom, new Chunks(no, no, 0, 0, 3,  3) },
                // two side neighbors
                { Left | Right, new Chunks(1,    2,  no, no, 1,  3) },
                { Top | Bottom, new Chunks(no,   no, 0,  0,  no, no) },
                { Left | Top, new Chunks(2,      2,  2,  1,  no, no) },
                { Right | Top, new Chunks(0,     2,  1,  1,  no, no) },
                { Left | Bottom, new Chunks(no,  no, 2,  0,  2,  3) },
                { Right | Bottom, new Chunks(no, no, 1,  0,  0,  3) },
                // three side neighbors
                { Left | Top | Bottom, new Chunks(no,   no, 2,  0,  no, no) },
                { Right | Top | Bottom, new Chunks(no,  no, 1,  0,  no, no) },
                { Left | Right | Top, new Chunks(1,     2,  no, no, no, no) },
                { Left | Right | Bottom, new Chunks(no, no, no, no, 1,  3) },
                // four side neighbors - no need for outline textures
            };
        }

        public static Chunks? GetChunk(NeighborsPattern neighbors)
        {
            if (PatternToChunk.TryGetValue(neighbors, out var result))
            {
                return result;
            }

            return null;
        }

        public struct Chunks
        {
            public readonly byte? BackColumn;

            public readonly byte? BackRow;

            public readonly byte? FrontColumn;

            public readonly byte? FrontRow;

            public readonly byte? SideColumn;

            public readonly byte? SideRow;

            public Chunks(
                byte? frontColumn,
                byte? frontRow,
                byte? sideColumn,
                byte? sideRow,
                byte? backColumn,
                byte? backRow)
            {
                this.FrontColumn = frontColumn;
                this.FrontRow = frontRow;
                this.SideColumn = sideColumn;
                this.SideRow = sideRow;
                this.BackColumn = backColumn;
                this.BackRow = backRow;
            }
        }
    }
}