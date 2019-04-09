namespace AtomicTorch.CBND.CoreMod.Noise
{
    public class CombinedNoiseSelector : INoiseSelector
    {
        private readonly INoiseSelector[] noiseSelectors;

        public CombinedNoiseSelector(params INoiseSelector[] noiseSelectors)
        {
            this.noiseSelectors = noiseSelectors;
        }

        public bool IsMatch(double x, double y, double rangeMultiplier)
        {
            foreach (var noise in this.noiseSelectors)
            {
                if (noise.IsMatch(x, y, rangeMultiplier))
                {
                    return true;
                }
            }

            return false;
        }
    }
}