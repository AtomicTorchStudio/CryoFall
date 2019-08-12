namespace AtomicTorch.CBND.CoreMod.Items
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface IProtoItemWithFuel : IProtoItem
    {
        IReadOnlyItemFuelConfig ItemFuelConfig { get; }

        bool ClientCanStartRefill(IItem item);

        void ClientOnRefilled(IItem item, bool isCurrentHotbarItem);
    }
}