namespace AtomicTorch.CBND.CoreMod.Items.Drones
{
    using System;

    public class ItemDroneControlStandard : ProtoItemDroneControl
    {
        public override string Description =>
            "Special remote control necessary to operate and issue commands to automated drones.";

        public override TimeSpan DurabilityLifetime => TimeSpan.FromHours(1);

        public override byte MaxDronesToControl => 3;

        public override string Name => "Drone remote control";
    }
}