namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowOilRefinery : BaseViewModel
    {
        public ViewModelWindowOilRefinery(
            IStaticWorldObject worldObject,
            ManufacturingState manufacturingState,
            ManufacturingState manufacturingStateProcessedGasoline,
            ManufacturingState manufacturingStateProcessedMineralOil,
            ManufacturingConfig manufacturingConfig,
            ManufacturingConfig manufacturingConfigProcessedGasoline,
            ManufacturingConfig manufacturingConfigProcessedMineralOil,
            FuelBurningState fuelBurningState,
            LiquidContainerState liquidStateRawPetroleum,
            LiquidContainerState liquidStateProcessedGasoline,
            LiquidContainerState liquidStateProcessedMineralOil,
            LiquidContainerConfig liquidConfigRawPetroleum,
            LiquidContainerConfig liquidConfigProcessedGasoline,
            LiquidContainerConfig liquidConfigProcessedMineralOil)
        {
            this.WorldObjectManufacturer = worldObject;

            // please note - the order of creating these view models is important for the proper container exchange order
            this.ViewModelFuelBurningState = new ViewModelFuelBurningState(fuelBurningState);
            this.ViewModelBurningFuel = ViewModelBurningFuel.Create(this.WorldObjectManufacturer, fuelBurningState);

            this.ViewModelManufacturingStateRawPetroleum = new ViewModelManufacturingState(
                worldObject,
                manufacturingState,
                manufacturingConfig);

            this.ViewModelManufacturingStateProcessedGasoline = new ViewModelManufacturingState(
                worldObject,
                manufacturingStateProcessedGasoline,
                manufacturingConfigProcessedGasoline);

            this.ViewModelManufacturingStateProcessedMineralOil = new ViewModelManufacturingState(
                worldObject,
                manufacturingStateProcessedMineralOil,
                manufacturingConfigProcessedMineralOil);

            this.ViewModelLiquidStateRawPetroleum = new ViewModelLiquidContainerState(
                liquidStateRawPetroleum,
                liquidConfigRawPetroleum);

            this.ViewModelLiquidStateProcessedGasoline = new ViewModelLiquidContainerState(
                liquidStateProcessedGasoline,
                liquidConfigProcessedGasoline);

            this.ViewModelLiquidStateProcessedMineralOil = new ViewModelLiquidContainerState(
                liquidStateProcessedMineralOil,
                liquidConfigProcessedMineralOil);

            // prepare active state property
            var manufacturerPublicState = worldObject.GetPublicState<ObjectManufacturerPublicState>();
            manufacturerPublicState.ClientSubscribe(_ => _.IsManufacturingActive,
                                                    _ => RefreshIsManufacturerActive(),
                                                    this);
            RefreshIsManufacturerActive();

            void RefreshIsManufacturerActive()
            {
                this.IsManufacturerActive = manufacturerPublicState.IsManufacturingActive;
            }
        }

        public ViewModelWindowOilRefinery()
        {
        }

        public bool IsManufacturerActive { get; private set; }

        public ViewModelBurningFuel ViewModelBurningFuel { get; }

        public ViewModelFuelBurningState ViewModelFuelBurningState { get; }

        public ViewModelLiquidContainerState ViewModelLiquidStateProcessedGasoline { get; }

        public ViewModelLiquidContainerState ViewModelLiquidStateProcessedMineralOil { get; }

        public ViewModelLiquidContainerState ViewModelLiquidStateRawPetroleum { get; }

        public ViewModelManufacturingState ViewModelManufacturingStateProcessedGasoline { get; }

        public ViewModelManufacturingState ViewModelManufacturingStateProcessedMineralOil { get; }

        public ViewModelManufacturingState ViewModelManufacturingStateRawPetroleum { get; }

        public IStaticWorldObject WorldObjectManufacturer { get; }
    }
}