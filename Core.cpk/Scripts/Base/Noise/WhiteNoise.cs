namespace AtomicTorch.CBND.CoreMod.Noise
{
    using System;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class WhiteNoise : ISimplexNoise
    {
        private readonly uint seed;

        public WhiteNoise(int seed)
        {
            this.seed = (uint)seed;
        }

        public double CombineWith(double current, double value)
        {
            throw new NotImplementedException();
        }

        public double Get(double x, double y)
        {
            return PositionHashHelper.GetHashDouble((uint)x, (uint)y, this.seed);
        }

        public double Get(double x, double y, double z)
        {
            throw new NotImplementedException();
        }
    }
}