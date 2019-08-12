namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    public enum ElectricityConsumerState : byte
    {
        PowerOff = 0,

        PowerOffOutage = 1,

        PowerOn = 128
    }
}