namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    public interface IProtoItemEquipmentImplant : IProtoItemEquipment
    {
        ushort BiomaterialAmountRequiredToInstall { get; }

        ushort BiomaterialAmountRequiredToUninstall { get; }
    }
}