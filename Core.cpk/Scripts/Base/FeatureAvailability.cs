namespace AtomicTorch.CBND.CoreMod
{
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum FeatureAvailability : byte
    {
        None = 0,

        All = 1,

        OnlyPvP = 2,

        OnlyPvE = 3
    }
}