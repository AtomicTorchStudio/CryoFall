namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using System;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class PositionalRandom
    {
        public static int Get(Vector2Ushort position, int minInclusive, int maxExclusive, uint seed = 0)
        {
            maxExclusive--;
            if (minInclusive == maxExclusive)
            {
                return minInclusive;
            }

            Api.Assert(minInclusive < maxExclusive, "Min should be < max");

            var value = PositionHashHelper.GetHashDouble(position.X, position.Y, seed);
            var range = maxExclusive - minInclusive;
            value = minInclusive + value * range;
            return (int)Math.Round(value, MidpointRounding.AwayFromZero);
        }
    }
}