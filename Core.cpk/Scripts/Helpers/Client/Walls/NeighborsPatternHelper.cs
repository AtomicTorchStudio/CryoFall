namespace AtomicTorch.CBND.CoreMod.Helpers.Client.Walls
{
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;

    public static class NeighborsPatternHelper
    {
        /// <summary>
        /// Cleanup pattern to remove impossible combinations.
        /// A combinations is considered as impossible if it contains diagonal neighbor without non-diagonal neighbors.
        /// For example, this combination is impossible:
        /// + + -
        /// + + -
        /// - - +
        /// Because bottom right direction doesn't have neighbor directions (right and bottom).
        /// It will be cleaned up to:
        /// + + -
        /// + + -
        /// - - -
        /// </summary>
        public static NeighborsPattern CleanupImpossibleCombinations(NeighborsPattern pattern)
        {
            if ((pattern & NeighborsPattern.TopLeft) == NeighborsPattern.TopLeft
                && (pattern & (NeighborsPattern.Left | NeighborsPattern.Top)) == 0)
            {
                // remove top left
                pattern &= ~NeighborsPattern.TopLeft;
            }

            if ((pattern & NeighborsPattern.TopRight) == NeighborsPattern.TopRight
                && (pattern & (NeighborsPattern.Top | NeighborsPattern.Right)) == 0)
            {
                // remove top right
                pattern &= ~NeighborsPattern.TopRight;
            }

            if ((pattern & NeighborsPattern.BottomRight) == NeighborsPattern.BottomRight
                && (pattern & (NeighborsPattern.Bottom | NeighborsPattern.Right)) == 0)
            {
                // remove bottom right
                pattern &= ~NeighborsPattern.BottomRight;
            }

            if ((pattern & NeighborsPattern.BottomLeft) == NeighborsPattern.BottomLeft
                && (pattern & (NeighborsPattern.Bottom | NeighborsPattern.Left)) == 0)
            {
                // remove bottom left
                pattern &= ~NeighborsPattern.BottomLeft;
            }

            return pattern;
        }
    }
}