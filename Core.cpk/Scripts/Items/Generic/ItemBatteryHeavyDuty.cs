namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    public class ItemBatteryHeavyDuty : ProtoItemBatteryDisposable
    {
        public override string Description =>
            "Heavy-duty disposable battery. Holds much more charge than smaller batteries and can be used to quickly recharge powerbanks or electronic devices.";

        public override double FuelAmount => 2500;

        public override string Name => "Heavy-duty battery";
    }
}