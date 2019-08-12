namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowWell : ViewModelWindowManufacturer
    {
        public ViewModelWindowWell()
        {
        }

        public ViewModelWindowWell(
            IStaticWorldObject worldObjectManufacturer,
            ObjectManufacturerPrivateState privateState,
            ManufacturingConfig manufacturingConfig,
            LiquidContainerState liquidContainerState,
            LiquidContainerConfig liquidContainerConfig)
            : base(
                worldObjectManufacturer,
                privateState,
                manufacturingConfig: manufacturingConfig)
        {
            this.CommandDrink = new ActionCommand(this.ExecuteCommandDrink);
            this.ViewModelLiquidContainerState = new ViewModelLiquidContainerState(
                liquidContainerState,
                liquidContainerConfig);
        }

        public BaseCommand CommandDrink { get; }

        public ViewModelLiquidContainerState ViewModelLiquidContainerState { get; }

        private void ExecuteCommandDrink()
        {
            ((ProtoObjectWell)this.WorldObjectManufacturer.ProtoStaticWorldObject)
                .ClientDrink(this.WorldObjectManufacturer);
        }
    }
}