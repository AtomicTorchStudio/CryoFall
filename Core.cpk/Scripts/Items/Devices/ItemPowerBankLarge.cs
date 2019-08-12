namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
    public class ItemPowerBankLarge : ProtoItemPowerBank
    {
        public override string Description =>
            "Stores large amounts of power for convenient use on the go. Any device or energy weapon can draw power from equipped powerbanks.";

        public override uint DurabilityMax => EnergyCapacity * 10;

        public override uint EnergyCapacity => 9000;

        public override string Name => "Large powerbank";
    }
}