namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum ElectricityConsumerState : byte
    {
        PowerOff = 0,

        PowerOnIdle = 1,

        PowerOnActive = 128
    }
}