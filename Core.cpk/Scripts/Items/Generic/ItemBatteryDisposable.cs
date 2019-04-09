namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemBatteryDisposable : ProtoItemBatteryDisposable
    {
        public override string Description =>
            "Simple disposable battery. Doesn't store a lot of power, but cheap and simple to produce. Can be used to recharge electronic devices or powerbanks.";

        public override double FuelAmount => 1000;

        public override string Name => "Disposable battery";
    }
}