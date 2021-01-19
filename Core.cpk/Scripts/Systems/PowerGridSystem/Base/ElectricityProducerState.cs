namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum ElectricityProducerState : byte
    {
        PowerOff = 0,

        PowerOnIdle = 128,

        PowerOnActive = 129
    }
}