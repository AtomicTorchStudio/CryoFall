namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelBurningFuel : BaseViewModel
    {
        private ViewModelBurningFuel(IStaticWorldObject worldObjectManufacturer, IFuelItemsContainer fuelItemsContainer)
        {
            // prepare active state property
            var manufacturerPublicState = worldObjectManufacturer.GetPublicState<ObjectManufacturerPublicState>();
            manufacturerPublicState.ClientSubscribe(_ => _.IsActive,
                                                    _ => RefreshIsManufacturerActive(),
                                                    this);
            RefreshIsManufacturerActive();

            void RefreshIsManufacturerActive()
            {
                this.IsActive = manufacturerPublicState.IsActive;
            }

            var (icon, color) = fuelItemsContainer.ClientGetFuelIconAndColor();
            this.FuelIcon = Client.UI.GetTextureBrush(icon);
            this.FuelColor = color;
        }

        public Color FuelColor { get; }

        public Brush FuelIcon { get; }

        public bool IsActive { get; set; }

        public static ViewModelBurningFuel Create(
            IStaticWorldObject worldObjectManufacturer,
            FuelBurningState fuelBurningState)
        {
            if (fuelBurningState != null
                && fuelBurningState.ContainerFuel?.ProtoItemsContainer is IFuelItemsContainer fuelItemsContainer)
            {
                return new ViewModelBurningFuel(worldObjectManufacturer, fuelItemsContainer);
            }

            return null;
        }
    }
}