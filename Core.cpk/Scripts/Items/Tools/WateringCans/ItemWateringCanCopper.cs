namespace AtomicTorch.CBND.CoreMod.Items.Tools.WateringCans
{
    public class ItemWateringCanCopper : ProtoItemToolWateringCan
    {
        public override string Description => "Classic copper design for a watering can. Can store more water.";

        public override ushort DurabilityMax => 300; // 6*50

        public override string Name => "Copper watering can";

        public override byte WaterCapacity => 6;
    }
}