namespace AtomicTorch.CBND.CoreMod.Items.Tools.Special
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;

    public interface IProtoItemVehicleRepairKit : IProtoItemTool, IProtoItemWithCharacterAppearance
    {
        ReadOnlySoundPreset<ObjectSound> ObjectInteractionSoundsPreset { get; }

        double RepairSpeedMultiplier { get; }
    }
}