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
        private readonly ObjectManufacturerPublicState manufacturerPublicState;

        public ViewModelWindowOilRefinery(
            IStaticWorldObject worldObject,
            ManufacturingState manufacturingState,
            ManufacturingState manufacturingStateProcessedGasoline,
            ManufacturingState manufacturingStateProcessedMineralOil,
            ManufacturingConfig manufacturingConfig,
            ManufacturingConfig manufacturingConfigProcessedGasoline,
            ManufacturingConfig manufacturingConfigProcessedMineralOil,
            LiquidContainerState liquidStateRawPetroleum,
            LiquidContainerState liquidStateProcessedGasoline,
            LiquidContainerState liquidStateProcessedMineralOil,
            LiquidContainerConfig liquidConfigRawPetroleum,
            LiquidContainerConfig liquidConfigProcessedGasoline,
            LiquidContainerConfig liquidConfigProcessedMineralOil)
        {
            this.WorldObjectManufacturer = worldObject;

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
            this.manufacturerPublicState = worldObject.GetPublicState<ObjectManufacturerPublicState>();
            this.manufacturerPublicState.ClientSubscribe(_ => _.IsActive,
                                                         _ => this.NotifyPropertyChanged(
                                                             nameof(this.IsManufacturingActive)),
                                                         this);
        }

        public ViewModelWindowOilRefinery()
        {
        }

        public bool IsManufacturingActive => this.manufacturerPublicState.IsActive;

        public ViewModelLiquidContainerState ViewModelLiquidStateProcessedGasoline { get; }

        public ViewModelLiquidContainerState ViewModelLiquidStateProcessedMineralOil { get; }

        public ViewModelLiquidContainerState ViewModelLiquidStateRawPetroleum { get; }

        public ViewModelManufacturingState ViewModelManufacturingStateProcessedGasoline { get; }

        public ViewModelManufacturingState ViewModelManufacturingStateProcessedMineralOil { get; }

        public ViewModelManufacturingState ViewModelManufacturingStateRawPetroleum { get; }

        public IStaticWorldObject WorldObjectManufacturer { get; }
    }
}