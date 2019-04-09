namespace AtomicTorch.CBND.CoreMod.Helpers.Client.Walls
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.DataStructures;

    public static class WallTextureChunkSelector
    {
        /// <summary>
        /// Returns wall chunk from texture atlas position based on wall neighbors pattern.
        /// </summary>
        public static WallChunkWithOverlays GetRegion(
            NeighborsPattern patterns,
            NeighborsPattern patternNeighbors)
        {
            var primary = GetRegion(patterns);
            var overlay = patternNeighbors == NeighborsPattern.None
                              ? null
                              : GetOverlayRegion(patternNeighbors);
            return new WallChunkWithOverlays(primary, overlay);
        }

        private static ITempList<WallPattern> GetOverlayRegion(NeighborsPattern patterns)
        {
            var result = Api.Shared.GetTempList<WallPattern>();
            foreach (var pattern in WallPatterns.PatternsOverlay)
            {
                if (pattern.IsPass(patterns))
                {
                    result.Add(pattern);
                }
            }

            return result;
        }

        private static WallPattern GetRegion(NeighborsPattern patterns)
        {
            foreach (var pattern in WallPatterns.PatternsPrimary)
            {
                if (pattern.IsPass(patterns))
                {
                    return pattern;
                }
            }

            throw new Exception("Not found!");
        }

        public struct WallChunkWithOverlays : IDisposable
        {
            public readonly WallPattern Primary;

            public ITempList<WallPattern> Overlay;

            public WallChunkWithOverlays(WallPattern primary, ITempList<WallPattern> overlay)
            {
                this.Primary = primary;
                this.Overlay = overlay;
            }

            public void Dispose()
            {
                this.Overlay?.Dispose();
                this.Overlay = null;
            }
        }
    }
}