namespace AtomicTorch.CBND.CoreMod.Items.Tools.WateringCans
{
    public class ItemWateringCanSteel : ProtoItemToolWateringCan
    {
        public override string Description =>
            "Made from thin machined sheets of stainless steel and quite light. Can store a lot of water.";

        public override ushort DurabilityMax => 450; // 9*50

        public override string Name => "Steel watering can";

        public override byte WaterCapacity => 9;
    }
}