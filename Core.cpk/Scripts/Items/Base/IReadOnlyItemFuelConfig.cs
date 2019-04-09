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

        double FuelUsePerSecond { get; }

        double RefillDuration { get; }

        string FuelTitle { get; }

        ITextureResource ClientGetFuelIcon();

        void ServerInitialize(ItemWithFuelPrivateState privateState, bool isFirstTimeInit);

        double SharedGetFuelAmount(IItem item);

        void SharedOnRefilled(IItem item, double fuelAmount);

        void SharedTryConsumeFuel(
            IItem item,
            ItemWithFuelPrivateState privateState,
            double deltaTime,
            out bool isFuelRanOut);
    }
}