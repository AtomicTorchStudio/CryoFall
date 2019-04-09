namespace AtomicTorch.CBND.CoreMod.Noise
{
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class NoiseSelector : INoiseSelector
    {
        private readonly ISimplexNoise noise;

        private readonly double rangeFrom;

        private readonly double rangeTo;

        public NoiseSelector(RangeDouble range, ISimplexNoise noise)
        {
            this.noise = noise;
            this.rangeFrom = range.From;
            this.rangeTo = range.To;
        }

        public NoiseSelector(double from, double to, ISimplexNoise noise)
            : this(new RangeDouble(from, to), noise)
        {
        }

        public bool IsMatch(double x, double y, double rangeMultiplier)
        {
            var noiseValue = this.noise.Get(x, y);
            var from = this.rangeFrom;
            var to = this.rangeTo;

            if (rangeMultiplier != 1.0)
            {
                var rangeDistance = to - from;
                var extraRange = rangeDistance * (1 - rangeMultiplier);
                from += extraRange / 2;
                to -= extraRange / 2;
            }

            return noiseValue >= from
                   && noiseValue <= to;
        }
    }
}