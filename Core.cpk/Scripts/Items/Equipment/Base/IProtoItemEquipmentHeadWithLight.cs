namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface IProtoItemEquipmentHeadWithLight : IProtoItemEquipmentHead, IProtoItemWithFuel
    {
        IReadOnlyItemLightConfig ItemLightConfig { get; }

        void ClientToggleLight(IItem item);
    }
}