namespace AtomicTorch.CBND.CoreMod.Helpers.Client.Floors
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client.Walls;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;

    public static class FloorTextureChunkSelector
    {
        /// <summary>
        /// Returns wall chunk from texture atlas position based on wall neighbors pattern.
        /// </summary>
        public static FloorChunkPreset GetRegion(NeighborsPattern pattern)
        {
            return GetRegion(pattern, FloorTextureComposer.FloorChunkPresets);
        }

        private static FloorChunkPreset GetRegion(
            NeighborsPattern pattern,
            IReadOnlyDictionary<NeighborsPattern, FloorChunkPreset> wallChunkTypes)
        {
            if (wallChunkTypes.TryGetValue(pattern, out var result))
            {
                // found matching atlas position
                return result;
            }

            // impossible combination - cleanup it and try again
            pattern = NeighborsPatternHelper.CleanupImpossibleCombinations(pattern);
            if (wallChunkTypes.TryGetValue(pattern, out result))
            {
                // found matching atlas position (after cleanup)
                return result;
            }

            // this should be impossible, but kept here for debugging purposes
            //Api.Logger.WriteError("Impossible pattern: " + patternSameType);
            return null;
        }
    }
}