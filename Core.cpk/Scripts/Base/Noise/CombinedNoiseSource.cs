namespace AtomicTorch.CBND.CoreMod.Noise
{
    using System;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class CombinedNoiseSource : ISimplexNoise
    {
        private readonly ISimplexNoise[] sources;

        public CombinedNoiseSource(ISimplexNoise[] sources)
        {
            if (sources.Length < 2)
            {
                throw new ArgumentException("At least two sources must be provided");
            }

            this.sources = sources;
        }

        public double CombineWith(double current, double value)
        {
            throw new Exception($"Cannot combine {nameof(CombinedNoiseSource)} with anything else");
        }

        public double Get(double x, double y)
        {
            var result = this.sources[0].Get(x, y);

            for (var index = 1; index < this.sources.Length; index++)
            {
                var source = this.sources[index];
                var value = source.Get(x, y);
                result = source.CombineWith(result, value);
            }

            return result;
        }

        public double Get(double x, double y, double z)
        {
            throw new NotImplementedException();
        }
    }
}