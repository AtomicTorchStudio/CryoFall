namespace AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    internal static class EditorTileOffsetsHelper
    {
        public static IEnumerable<Vector2Int> GenerateOffsets(EditorBrushShape brush, int brushSize, bool isForPreview)
        {
            switch (brush)
            {
                case EditorBrushShape.Circle:
                    return ShapeTileOffsetsHelper.GenerateOffsetsCircle(brushSize);

                case EditorBrushShape.Square:
                    return ShapeTileOffsetsHelper.GenerateOffsetsSquare(brushSize);

                case EditorBrushShape.Spray:
                    return isForPreview
                               ? ShapeTileOffsetsHelper.GenerateOffsetsSquare(brushSize)
                               : ShapeTileOffsetsHelper.GenerateOffsetsSpray(brushSize);
            }

            throw new ArgumentOutOfRangeException(nameof(brush), "Unknown brush shape");
        }
    }
}