namespace AtomicTorch.CBND.CoreMod.Helpers.Client.Floors
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using static Primitives.NeighborsPattern;

    public static class FloorSpritePatterns
    {
        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        public static List<FloorPatternLayer> CreatePatternsForPrimaryLayers()
        {
            var patterns = new List<FloorPatternLayer>();

            patterns.Add(
                new FloorPatternLayer(
                    "Default",
                    sourceChunk: new FloorSourceChunk(0, 0),
                    requires: None,
                    excludes: None));

            // sides borders
            patterns.Add(
                new FloorPatternLayer(
                    "Top side",
                    sourceChunk: new FloorSourceChunk(4, 0),
                    requires: None,
                    excludes: Top));

            patterns.Add(
                new FloorPatternLayer(
                    "Left side",
                    sourceChunk: new FloorSourceChunk(1, 0),
                    requires: None,
                    excludes: Left));

            patterns.Add(
                new FloorPatternLayer(
                    "Right side",
                    sourceChunk: new FloorSourceChunk(2, 0),
                    requires: None,
                    excludes: Right));

            patterns.Add(
                new FloorPatternLayer(
                    "Bottom side",
                    sourceChunk: new FloorSourceChunk(3, 0),
                    requires: None,
                    excludes: Bottom));

            // inner corners borders - when no diagonal neighbor of the same type
            patterns.Add(
                new FloorPatternLayer(
                    "Top-left inner corner",
                    sourceChunk: new FloorSourceChunk(0, 1),
                    requires: Top | Left,
                    excludes: TopLeft));

            patterns.Add(
                new FloorPatternLayer(
                    "Top-right inner corner",
                    sourceChunk: new FloorSourceChunk(1, 1),
                    requires: Top | Right,
                    excludes: TopRight));

            patterns.Add(
                new FloorPatternLayer(
                    "Bottom-left inner corner",
                    sourceChunk: new FloorSourceChunk(2, 1),
                    requires: Bottom | Left,
                    excludes: BottomLeft));

            patterns.Add(
                new FloorPatternLayer(
                    "Bottom-right inner corner",
                    sourceChunk: new FloorSourceChunk(3, 1),
                    requires: Bottom | Right,
                    excludes: BottomRight));

            // outer corners borders
            patterns.Add(
                new FloorPatternLayer(
                    "Top-left outer corner",
                    sourceChunk: new FloorSourceChunk(0, 1),
                    requires: None,
                    excludes: Top | Left));

            patterns.Add(
                new FloorPatternLayer(
                    "Top-right outer corner",
                    sourceChunk: new FloorSourceChunk(1, 1),
                    requires: None,
                    excludes: Top | Right));

            patterns.Add(
                new FloorPatternLayer(
                    "Bottom-left outer corner",
                    sourceChunk: new FloorSourceChunk(2, 1),
                    requires: None,
                    excludes: Bottom | Left));

            patterns.Add(
                new FloorPatternLayer(
                    "Bottom-right outer corner",
                    sourceChunk: new FloorSourceChunk(3, 1),
                    requires: None,
                    excludes: Bottom | Right));

            return patterns;
        }
    }
}