namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    public enum ElectricityConsumerState : byte
    {
        PowerOff = 0,

        PowerOnIdle = 1,

        PowerOnActive = 128
    }
}