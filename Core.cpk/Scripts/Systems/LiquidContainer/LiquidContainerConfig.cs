namespace AtomicTorch.CBND.CoreMod.Systems.LiquidContainer
{
    public class LiquidContainerConfig
    {
        public readonly double AmountAutoDecreasePerSecondWhenUse;

        public readonly double AmountAutoIncreasePerSecond;

        public readonly double Capacity;

        public LiquidContainerConfig(
            double capacity,
            double amountAutoIncreasePerSecond,
            double amountAutoDecreasePerSecondWhenUse)
        {
            this.AmountAutoIncreasePerSecond = amountAutoIncreasePerSecond;
            this.AmountAutoDecreasePerSecondWhenUse = amountAutoDecreasePerSecondWhenUse;
            this.Capacity = capacity;
        }
    }
}