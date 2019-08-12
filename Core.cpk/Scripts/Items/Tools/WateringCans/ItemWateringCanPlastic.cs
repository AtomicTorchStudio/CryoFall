namespace AtomicTorch.CBND.CoreMod.Items.Tools.WateringCans
{
    public class ItemWateringCanPlastic : ProtoItemToolWateringCan
    {
        public override string Description =>
            "Simple watering can made from lightweight plastic. Holds the most water of all watering cans.";

        public override uint DurabilityMax => 600; // 12*50

        public override string Name => "Plastic watering can";

        public override byte WaterCapacity => 12;
    }
}