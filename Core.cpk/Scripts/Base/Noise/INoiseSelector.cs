namespace AtomicTorch.CBND.CoreMod.Noise
{
    public interface INoiseSelector
    {
        bool IsMatch(double x, double y, double rangeMultiplier);
    }
}