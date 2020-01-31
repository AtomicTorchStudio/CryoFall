namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowManufacturer : BaseViewModel
    {
        private readonly ObjectManufacturerPublicState publicState;

        public ViewModelWindowManufacturer(
            IStaticWorldObject worldObjectManufacturer,
            ObjectManufacturerPrivateState privateState,
            ManufacturingConfig manufacturingConfig)
        {
            this.WorldObjectManufacturer = worldObjectManufacturer;
            var fuelBurningState = privateState.FuelBurningState;
            var manufacturingState = privateState.ManufacturingState;

            // please note - the order of creating these view models is important for the proper container exchange order
            this.ViewModelFuelBurningState = fuelBurningState != null
                                                 ? new ViewModelFuelBurningState(fuelBurningState)
                                                 : null;

            this.ViewModelManufacturingState = new ViewModelManufacturingState(
                worldObjectManufacturer,
                manufacturingState,
                manufacturingConfig);

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

            this.publicState = worldObjectManufacturer.GetPublicState<ObjectManufacturerPublicState>();
            this.publicState.ClientSubscribe(_ => _.IsActive,
                                             _ => this.NotifyPropertyChanged(nameof(this.IsManufacturingActive)),
                                             this);
        }

        public ViewModelWindowManufacturer()
        {
        }

        public bool IsManufacturingActive => this.publicState.IsActive;

        public bool IsNeedFuel { get; private set; }

        public string Title => this.WorldObjectManufacturer.ProtoGameObject.Name;

        public VerticalAlignment VerticalAlignment
            => VerticalAlignment.Center;

        public ViewModelBurningFuel ViewModelBurningFuel { get; }

        public ViewModelFuelBurningState ViewModelFuelBurningState { get; }

        public ViewModelManufacturingState ViewModelManufacturingState { get; }

        public Visibility VisibilityElectricityControls
            => this.ViewModelFuelBurningState == null
               && this.WorldObjectManufacturer.ProtoStaticWorldObject is IProtoObjectElectricityConsumer protoConsumer
               && protoConsumer.ElectricityConsumptionPerSecondWhenActive > 0
                   ? Visibility.Visible
                   : Visibility.Collapsed;

        public Visibility VisibilityFuelControls
            => this.ViewModelFuelBurningState != null
                   ? Visibility.Visible
                   : Visibility.Collapsed;

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