namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowManufacturer : BaseViewModel
    {
        public ViewModelWindowManufacturer(
            IStaticWorldObject worldObjectManufacturer,
            ManufacturingState manufacturingState,
            ManufacturingConfig manufacturingConfig,
            FuelBurningState fuelBurningState)
        {
            this.WorldObjectManufacturer = worldObjectManufacturer;

            // please note - the order of creating these view models is important for the proper container exchange order
            this.ViewModelFuelBurningState = fuelBurningState != null
                                                 ? new ViewModelFuelBurningState(fuelBurningState)
                                                 : null;

            this.ViewModelManufacturingState = new ViewModelManufacturingState(
                worldObjectManufacturer,
                manufacturingState,
                manufacturingConfig);

            this.VisibilityFuelControls = this.ViewModelFuelBurningState != null
                                              ? Visibility.Visible
                                              : Visibility.Collapsed;

            this.ViewModelBurningFuel = ViewModelBurningFuel.Create(worldObjectManufacturer, fuelBurningState);

            this.ViewModelManufacturingState.SubscribePropertyChange(
                _ => _.SelectedRecipe,
                this.RefreshIsNeedFuel);

            this.ViewModelManufacturingState.SubscribePropertyChange(
                _ => _.IsInputMatchSelectedRecipe,
                this.RefreshIsNeedFuel);

            this.ViewModelBurningFuel?.SubscribePropertyChange(
                _ => _.IsActive,
                this.RefreshIsNeedFuel);

            this.ViewModelFuelBurningState?.SubscribePropertyChange(
                _ => _.FuelUsageCurrentValue,
                this.RefreshIsNeedFuel);

            this.ViewModelManufacturingState.ContainerInput.StateHashChanged += this.ContainerInputStateChanged;

            this.RefreshIsNeedFuel();
        }

        public ViewModelWindowManufacturer()
        {
        }

        public bool IsNeedFuel { get; private set; }

        public string Title => this.WorldObjectManufacturer.ProtoGameObject.Name;

        public VerticalAlignment VerticalAlignment
            => this.ViewModelFuelBurningState != null
                   ? VerticalAlignment.Top
                   : VerticalAlignment.Center;

        public ViewModelBurningFuel ViewModelBurningFuel { get; }

        public ViewModelFuelBurningState ViewModelFuelBurningState { get; }

        public ViewModelManufacturingState ViewModelManufacturingState { get; }

        public Visibility VisibilityFuelControls { get; } = Visibility.Visible;

        public IStaticWorldObject WorldObjectManufacturer { get; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.ViewModelManufacturingState.ContainerInput.StateHashChanged -= this.ContainerInputStateChanged;
        }

        private void ContainerInputStateChanged()
        {
            this.RefreshIsNeedFuel();
        }

        private void RefreshIsNeedFuel()
        {
            if (this.ViewModelBurningFuel == null
                || this.ViewModelFuelBurningState == null)
            {
                this.IsNeedFuel = false;
                return;
            }

            this.IsNeedFuel = !this.ViewModelBurningFuel.IsActive
                              && this.ViewModelManufacturingState.SelectedRecipe != null
                              && this.ViewModelManufacturingState.ContainerInput.OccupiedSlotsCount > 0
                              && this.ViewModelManufacturingState.IsInputMatchSelectedRecipe
                              && this.ViewModelFuelBurningState.FuelUsageCurrentValue <= 0;
        }
    }
}