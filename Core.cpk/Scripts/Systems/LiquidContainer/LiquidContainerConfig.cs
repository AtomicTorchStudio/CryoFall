namespace AtomicTorch.CBND.CoreMod.Systems.LiquidContainer
{
    public class LiquidContainerConfig
    {
        public readonly float AmountAutoDecreasePerSecondWhenUse;

        public readonly float AmountAutoIncreasePerSecond;

        public readonly float Capacity;

        public LiquidContainerConfig(
            float capacity,
            float amountAutoIncreasePerSecond,
            float amountAutoDecreasePerSecondWhenUse)
        {
            this.AmountAutoIncreasePerSecond = amountAutoIncreasePerSecond;
            this.AmountAutoDecreasePerSecondWhenUse = amountAutoDecreasePerSecondWhenUse;
            this.Capacity = capacity;
        }
    }
}