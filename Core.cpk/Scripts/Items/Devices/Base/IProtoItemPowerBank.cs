namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
    using AtomicTorch.CBND.CoreMod.Items.Equipment;

    public interface IProtoItemPowerBank : IProtoItemEquipmentDevice
    {
        uint EnergyCapacity { get; }
    }
}