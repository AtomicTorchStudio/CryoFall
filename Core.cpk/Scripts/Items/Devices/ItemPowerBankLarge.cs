namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
    public class ItemPowerBankLarge : ProtoItemPowerBank
    {
        public override string Description =>
            "Stores large amounts of power for convenient use on the go. Any device or energy weapon can draw power from equipped powerbanks.";

        public override ushort DurabilityMax => 60000;

        public override uint EnergyCapacity => 6000;

        public override string Name => "Large powerbank";
    }
}