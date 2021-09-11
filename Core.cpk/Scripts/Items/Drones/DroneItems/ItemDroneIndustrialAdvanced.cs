namespace AtomicTorch.CBND.CoreMod.Items.Drones
{
    using AtomicTorch.CBND.CoreMod.Drones;

    public class ItemDroneIndustrialAdvanced : ProtoItemDrone<DroneIndustrialAdvanced>
    {
        public override string Description =>
            "Advanced version of the industrial drone offers increased mining speed and range, as well as other improvements.";

        public override uint DurabilityMax => 10000;

        public override string Name => "Advanced industrial drone";

        public override int SelectionOrder => 200;
    }
}