namespace AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem
{
    public enum ElectricityProducerState : byte
    {
        PowerOff = 0,

        PowerOnIdle = 128,

        PowerOnActive = 129
    }
}