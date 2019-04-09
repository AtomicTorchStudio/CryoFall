namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowBarrel : ViewModelWindowManufacturer
    {
        private readonly ProtoBarrelPrivateState privateState;

        public ViewModelWindowBarrel(
            IStaticWorldObject worldObjectManufacturer,
            ManufacturingState manufacturingState,
            ManufacturingConfig manufacturingConfig)
            : base(
                worldObjectManufacturer,
                manufacturingState,
                manufacturingConfig,
                null)
        {
            var protoBarrel = (IProtoObjectBarrel)worldObjectManufacturer.ProtoStaticWorldObject;
            this.LiquidCapacity = protoBarrel.LiquidCapacity;

            this.privateState = protoBarrel.GetBarrelPrivateState(worldObjectManufacturer);
            this.privateState.ClientSubscribe(
                _ => _.LiquidAmount,
                _ => this.RefreshLiquidAmount(),
                this);

            this.privateState.ClientSubscribe(
                _ => _.LiquidType,
                _ => this.RefreshLiquidType(),
                this);

            manufacturingState.ClientSubscribe(
                _ => _.SelectedRecipe,
                _ => this.RefreshRecipe(),
                this);

            this.RefreshLiquidAmount();
            this.RefreshRecipe();

            // no need - automatically refreshed with the recipe refresh
            //this.RefreshLiquidType();
        }

        public bool IsProgressBarInverted { get; set; }

        public ushort LiquidAmount { get; set; }

        public ushort LiquidCapacity { get; }

        public Color LiquidColor { get; set; } = Colors.Orange;

        public Brush LiquidIcon { get; set; } = IsDesignTime ? Brushes.Orange : null;

        public Visibility ProgressBarVisibility { get; set; } = Visibility.Collapsed;

        private void RefreshLiquidAmount()
        {
            this.LiquidAmount = this.privateState.LiquidAmount;
        }

        private void RefreshLiquidType()
        {
            var recipeInputLiquid =
                this.ViewModelManufacturingState.SelectedRecipe?.InputItems[0].ProtoItem as IProtoItemLiquidStorage;
            var liquidType = recipeInputLiquid?.LiquidType
                             ?? this.privateState.LiquidType;

            (this.LiquidColor, this.LiquidIcon) = LiquidColorIconHelper.GetColorAndIcon(liquidType);
        }

        private void RefreshRecipe()
        {
            var recipe = this.ViewModelManufacturingState.SelectedRecipe;
            this.IsProgressBarInverted = recipe is IRecipeBarrelAddLiquid;
            this.ProgressBarVisibility = recipe != null ? Visibility.Visible : Visibility.Hidden;
            this.RefreshLiquidType();
        }
    }
}