namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IFuelItemsContainer : IProtoItemsContainer
    {
        Type FuelType { get; }

        (ITextureResource icon, Color color) ClientGetFuelIconAndColor();
    }
}