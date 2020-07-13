namespace AtomicTorch.CBND.CoreMod.Items.Drones
{
    using System;

    public class ItemDroneControlAdvanced : ProtoItemDroneControl
    {
        public override string Description => GetProtoEntity<ItemDroneControlStandard>().Description;

        public override TimeSpan DurabilityLifetime => TimeSpan.FromHours(1);

        public override byte MaxDronesToControl => 4;

        public override string Name => "Advanced drone remote control";
    }
}