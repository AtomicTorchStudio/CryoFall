namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowOilCrackingPlant : BaseViewModel
    {
        private readonly ObjectManufacturerPublicState manufacturerPublicState;

        public ViewModelWindowOilCrackingPlant(
            IStaticWorldObject worldObject,
            ManufacturingState manufacturingState,
            ManufacturingState manufacturingStateProcessedGasoline,
            ManufacturingConfig manufacturingConfig,
            ManufacturingConfig manufacturingConfigProcessedGasoline,
            LiquidContainerState liquidStateMineralOil,
            LiquidContainerState liquidStateProcessedGasoline,
            LiquidContainerConfig liquidConfigMineralOil,
            LiquidContainerConfig liquidConfigProcessedGasoline)
        {
            this.WorldObjectManufacturer = worldObject;

            this.ViewModelManufacturingStateMineralOil = new ViewModelManufacturingState(
                worldObject,
                manufacturingState,
                manufacturingConfig);

            this.ViewModelManufacturingStateProcessedGasoline = new ViewModelManufacturingState(
                worldObject,
                manufacturingStateProcessedGasoline,
                manufacturingConfigProcessedGasoline);

            this.ViewModelLiquidStateMineralOil = new ViewModelLiquidContainerState(
                liquidStateMineralOil,
                liquidConfigMineralOil);

            this.ViewModelLiquidStateProcessedGasoline = new ViewModelLiquidContainerState(
                liquidStateProcessedGasoline,
                liquidConfigProcessedGasoline);

            // prepare active state property
            this.manufacturerPublicState = worldObject.GetPublicState<ObjectManufacturerPublicState>();
            this.manufacturerPublicState.ClientSubscribe(_ => _.IsActive,
                                                         _ => this.NotifyPropertyChanged(
                                                             nameof(this.IsManufacturingActive)),
                                                         this);
        }

        public ViewModelWindowOilCrackingPlant()
        {
        }

        public bool IsManufacturingActive => this.manufacturerPublicState.IsActive;

        public ViewModelLiquidContainerState ViewModelLiquidStateMineralOil { get; }

        public ViewModelLiquidContainerState ViewModelLiquidStateProcessedGasoline { get; }

        public ViewModelManufacturingState ViewModelManufacturingStateMineralOil { get; }

        public ViewModelManufacturingState ViewModelManufacturingStateProcessedGasoline { get; }

        public IStaticWorldObject WorldObjectManufacturer { get; }
    }
}