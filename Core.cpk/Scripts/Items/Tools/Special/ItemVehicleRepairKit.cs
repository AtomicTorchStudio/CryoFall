namespace AtomicTorch.CBND.CoreMod.Items.Tools.Special
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public class ItemVehicleRepairKit : ProtoItemVehicleRepairKit
    {
        public override string Description => "Universal disposable vehicle repair kit.";

        public override uint DurabilityMax => 5; // 5 times, each +20%, same as repairing in the VAB

        public override bool IsRepairable => false;

        public override string Name => "Vehicle repair kit";

        public override double RepairSpeedMultiplier => 1;

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetToolbox()
        {
            return ObjectsSoundsPresets.ObjectConstructionSite.Clone()
                                       .Replace(ObjectSound.InteractProcess, "Items/Tools/VehicleRepairKit/Process");
        }
    }
}