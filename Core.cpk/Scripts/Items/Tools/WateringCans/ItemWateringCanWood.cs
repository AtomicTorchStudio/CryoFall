namespace AtomicTorch.CBND.CoreMod.Items.Tools.WateringCans
{
    public class ItemWateringCanWood : ProtoItemToolWateringCan
    {
        public override string Description => "Basic watering can. Quite crudely made, so it doesn't hold much water.";

        public override ushort DurabilityMax => 90; // 3*30

        public override string Name => "Wooden watering can";

        public override byte WaterCapacity => 3;
    }
}