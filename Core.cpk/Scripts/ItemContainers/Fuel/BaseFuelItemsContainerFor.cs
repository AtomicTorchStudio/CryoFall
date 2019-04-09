namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Resources;

    public abstract class BaseFuelItemsContainerFor<TProtoItemFuel>
        : BaseItemsContainerFor<TProtoItemFuel>, IFuelItemsContainer
        where TProtoItemFuel : IProtoItemFuel
    {
        public Type FuelType => typeof(TProtoItemFuel);

        public (ITextureResource icon, Color color) ClientGetFuelIconAndColor()
        {
            return ProtoItemFuelIconColorHelper.GetIconAndColor(this.FuelType);
        }
    }
}