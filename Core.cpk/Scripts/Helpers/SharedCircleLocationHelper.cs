namespace AtomicTorch.CBND.CoreMod.Helpers
{
    using System;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class SharedCircleLocationHelper
    {
        public static Vector2Ushort SharedSelectRandomPositionInsideTheCircle(
            Vector2Ushort circlePosition,
            ushort circleRadius)
        {
            return SharedSelectRandomPositionInsideTheCircle(
                    circlePosition.ToVector2D(),
                    circleRadius)
                .ToVector2Ushort();
        }

        public static Vector2Ushort SharedSelectRandomPositionInsideTheCircle(
            Vector2Ushort circlePosition,
            ushort circleRadiusMin,
            ushort circleRadiusMax)
        {
            return SharedSelectRandomPositionInsideTheCircle(
                    circlePosition.ToVector2D(),
                    circleRadiusMin,
                    circleRadiusMax)
                .ToVector2Ushort();
        }

        public static Vector2D SharedSelectRandomPositionInsideTheCircle(
            Vector2D circlePosition,
            double circleRadius)
        {
            var offset = circleRadius * RandomHelper.NextDouble();
            var angle = RandomHelper.NextDouble() * MathConstants.DoublePI;
            return new Vector2D(circlePosition.X + offset * Math.Cos(angle),
                                circlePosition.Y + offset * Math.Sin(angle));
        }

        public static Vector2D SharedSelectRandomPositionInsideTheCircle(
            Vector2D circlePosition,
            double circleRadiusMin,
            double circleRadiusMax)
        {
            var deltaRadius = circleRadiusMax - circleRadiusMin;
            if (deltaRadius < 0)
            {
                Api.Logger.Error("circleRadiusMax is less than circleRadiusMin");
                deltaRadius = 0;
            }

            var offset = circleRadiusMin + deltaRadius * RandomHelper.NextDouble();
            var angle = RandomHelper.NextDouble() * MathConstants.DoublePI;
            return new Vector2D(circlePosition.X + offset * Math.Cos(angle),
                                circlePosition.Y + offset * Math.Sin(angle));
        }
    }
}