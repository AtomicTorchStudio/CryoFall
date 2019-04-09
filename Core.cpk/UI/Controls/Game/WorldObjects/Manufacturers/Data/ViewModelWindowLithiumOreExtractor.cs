namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data
{
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowLithiumOreExtractor : ViewModelWindowManufacturer
    {
        public ViewModelWindowLithiumOreExtractor(
            IStaticWorldObject worldObjectManufacturer,
            IStaticWorldObject worldObjectDeposit,
            ManufacturingState manufacturingState,
            ManufacturingConfig manufacturingConfig,
            FuelBurningState fuelBurningState,
            LiquidContainerState liquidContainerState,
            LiquidContainerConfig liquidContainerConfig)
            : base(worldObjectManufacturer,
                   manufacturingState,
                   manufacturingConfig,
                   fuelBurningState)
        {
            this.ViewModelLiquidContainerState = new ViewModelLiquidContainerState(liquidContainerState,
                                                                                   liquidContainerConfig);

            this.ViewModelDepositCapacityStatsControl = new ViewModelDepositCapacityStatsControl(worldObjectDeposit);
        }

        public ViewModelDepositCapacityStatsControl ViewModelDepositCapacityStatsControl { get; }

        public ViewModelLiquidContainerState ViewModelLiquidContainerState { get; }
    }
}