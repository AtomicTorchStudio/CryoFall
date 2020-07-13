namespace AtomicTorch.CBND.CoreMod.Items.Drones
{
    using AtomicTorch.CBND.CoreMod.Drones;

    public class ItemDroneIndustrialStandard : ProtoItemDrone<DroneIndustrialStandard>
    {
        public override string Description => "Standard industrial drone can be used for quick resource acquisition.";

        public override uint DurabilityMax => 6000;

        public override string Name => "Industrial drone";

        public override int SelectionOrder => 100;
    }
}