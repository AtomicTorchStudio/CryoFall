namespace AtomicTorch.CBND.CoreMod.Noise
{
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum NoiseCombineMode : byte
    {
        Max,

        Average,

        Add,

        Multiply
    }
}