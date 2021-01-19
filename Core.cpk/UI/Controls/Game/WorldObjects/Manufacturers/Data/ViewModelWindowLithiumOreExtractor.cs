namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowLithiumOreExtractor : ViewModelWindowManufacturer
    {
        public ViewModelWindowLithiumOreExtractor(
            IStaticWorldObject worldObjectManufacturer,
            IStaticWorldObject worldObjectDeposit,
            ObjectManufacturerPrivateState privateState,
            ManufacturingConfig manufacturingConfig,
            LiquidContainerState liquidContainerState,
            LiquidContainerConfig liquidContainerConfig)
            : base(worldObjectManufacturer,
                   privateState,
                   manufacturingConfig)
        {
            this.ViewModelLiquidContainerState = new ViewModelLiquidContainerState(liquidContainerState,
                liquidContainerConfig);

            this.ViewModelDepositCapacityStatsControl
                = new ViewModelDepositCapacityStatsControl(worldObjectDeposit,
                                                           worldObjectManufacturer.TilePosition);
        }

        public ViewModelDepositCapacityStatsControl ViewModelDepositCapacityStatsControl { get; }

        public ViewModelLiquidContainerState ViewModelLiquidContainerState { get; }
    }
}