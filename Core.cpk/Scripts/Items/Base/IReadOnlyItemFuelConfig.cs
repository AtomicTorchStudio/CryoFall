namespace AtomicTorch.CBND.CoreMod.Items
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IReadOnlyItemFuelConfig
    {
        double FuelAmountInitial { get; }

        double FuelCapacity { get; }

        ITextureResource FuelCustomIcon { get; set; }

        IReadOnlyList<IProtoItem> FuelProtoItemsList { get; }

        string FuelTitle { get; }

        double FuelUsePerSecond { get; }

        bool IsElectricity { get; }

        double RefillDuration { get; }

        ITextureResource ClientGetFuelIcon();

        void ServerInitialize(ItemWithFuelPrivateState privateState, bool isFirstTimeInit);

        double SharedGetFuelAmount(IItem item);

        void SharedOnRefilled(
            IItem item,
            double newFuelAmount,
            bool serverNotifyClients);

        void SharedTryConsumeFuel(
            IItem item,
            ItemWithFuelPrivateState privateState,
            double deltaTime,
            out bool isFuelRanOut);
    }
}