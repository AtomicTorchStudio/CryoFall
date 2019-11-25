namespace AtomicTorch.CBND.CoreMod.Helpers
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ShapeTileOffsetsHelper
    {
        public static IEnumerable<Vector2Int> GenerateOffsetsCircle(int brushSize)
        {
            if (brushSize <= 2)
            {
                foreach (var offset in GenerateOffsetsSquare(brushSize))
                {
                    yield return offset;
                }

                yield break;
            }

            var startX = -(brushSize - 1) / 2;
            var startY = startX;

            // ReSharper disable once PossibleLossOfFraction
            double center = brushSize / 2;

            double radiusSquared;
            switch (brushSize)
            {
                case 3:
                    radiusSquared = 2;
                    break;

                case 4:
                    center = 1.5;
                    radiusSquared = 4.25;
                    break;

                case 6:
                    center = 2.5;
                    radiusSquared = 8;
                    break;

                default:
                    radiusSquared = brushSize / 2d;
                    radiusSquared *= radiusSquared;
                    break;
            }

            for (var x = 0; x < brushSize; x++)
            for (var y = 0; y < brushSize; y++)
            {
                var dx = center - x;
                var dy = center - y;
                var distanceSquared = dx * dx + dy * dy;

                if (distanceSquared < radiusSquared)
                {
                    yield return new Vector2Int(x + startX, y + startY);
                }
            }
        }

        public static IEnumerable<Vector2Int> GenerateOffsetsSpray(int brushSize)
        {
            var startX = -(brushSize - 1) / 2;
            var startY = startX;

            var random = new Random();

            for (var x = 0; x < brushSize; x++)
            for (var y = 0; y < brushSize; y++)
            {
                if (random.Next(0, brushSize * 2) == 0)
                {
                    yield return new Vector2Int(x + startX, y + startY);
                }
            }
        }

        public static IEnumerable<Vector2Int> GenerateOffsetsSquare(int brushSize)
        {
            var startX = -(brushSize - 1) / 2;
            var startY = startX;

            for (var x = 0; x < brushSize; x++)
            for (var y = 0; y < brushSize; y++)
            {
                yield return new Vector2Int(x + startX, y + startY);
            }
        }

        public static Vector2Int[] SelectOffsetsWithRate(Vector2Int[] source, int rate)
        {
            var resultLength = source.Length / rate;
            var result = new Vector2Int[resultLength];
            for (var index = 0; index < resultLength; index++)
            {
                result[index] = source[index * rate];
            }

            return result;
        }
    }
}