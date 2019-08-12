namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
    public class ItemPowerBankStandard : ProtoItemPowerBank
    {
        public override string Description =>
            "Standard bidirectional powerbank. Conveniently stores energy for use on the go. Any device or energy weapon can draw power from equipped powerbanks.";

        public override uint DurabilityMax => EnergyCapacity * 10;

        public override uint EnergyCapacity => 3000;

        public override string Name => "Standard powerbank";
    }
}