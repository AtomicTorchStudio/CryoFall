namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelWindowBarrel : ViewModelWindowManufacturer
    {
        private readonly NetworkSyncList<CraftingQueueItem> craftingQueueItems;

        private readonly ProtoBarrelPrivateState privateState;

        private readonly IProtoObjectBarrel protoBarrel;

        public ViewModelWindowBarrel(
            IStaticWorldObject worldObjectManufacturer,
            ObjectManufacturerPrivateState privateState,
            ManufacturingConfig manufacturingConfig)
            : base(
                worldObjectManufacturer,
                privateState,
                manufacturingConfig)
        {
            this.protoBarrel = (IProtoObjectBarrel)worldObjectManufacturer.ProtoStaticWorldObject;
            this.LiquidCapacity = this.protoBarrel.LiquidCapacity;

            this.privateState = this.protoBarrel.GetBarrelPrivateState(worldObjectManufacturer);
            this.privateState.ClientSubscribe(
                _ => _.LiquidAmount,
                _ => this.RefreshLiquidAmount(),
                this);

            this.privateState.ClientSubscribe(
                _ => _.LiquidType,
                _ => this.RefreshLiquidType(),
                this);

            var manufacturingState = privateState.ManufacturingState;
            manufacturingState.ClientSubscribe(
                _ => _.SelectedRecipe,
                _ => this.RefreshRecipe(),
                this);

            this.craftingQueueItems = manufacturingState.CraftingQueue.QueueItems;
            this.craftingQueueItems.ClientAnyModification += this.QueueItemsClientAnyModificationHandler;

            this.RefreshLiquidAmount();
            this.RefreshRecipe();

            // no need - automatically refreshed with the recipe refresh
            //this.RefreshLiquidType();
        }

        public BaseCommand CommandDrain
            => new ActionCommand(this.ExecuteCommandDrain);

        public bool IsProgressBarInverted { get; set; }

        public ushort LiquidAmount { get; set; }

        public ushort LiquidCapacity { get; }

        public Color LiquidColor { get; set; } = Colors.Orange;

        public Brush LiquidIcon { get; set; } = IsDesignTime ? Brushes.Orange : null;

        public string LiquidTitle { get; set; }

        public Visibility ProgressBarVisibility { get; set; } = Visibility.Collapsed;

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.craftingQueueItems.ClientAnyModification -= this.QueueItemsClientAnyModificationHandler;
        }

        private void ExecuteCommandDrain()
        {
            DialogWindow.ShowDialog(
                CoreStrings.QuestionAreYouSure,
                CoreStrings.Button_Drain_DialogConfirmation,
                okText: CoreStrings.Yes,
                cancelText: CoreStrings.Button_Cancel,
                focusOnCancelButton: true,
                okAction: () => this.protoBarrel.ClientDrainBarrel(this.WorldObjectManufacturer),
                cancelAction: () => { });
        }

        private void QueueItemsClientAnyModificationHandler(NetworkSyncList<CraftingQueueItem> source)
        {
            this.RefreshRecipe();
        }

        private void RefreshLiquidAmount()
        {
            this.LiquidAmount = this.privateState.LiquidAmount;
            this.RefreshRecipe();
        }

        private void RefreshLiquidType()
        {
            IProtoItemLiquidStorage recipeInputLiquid;
            if (!this.ViewModelManufacturingState.ManufacturingState.HasActiveRecipe)
            {
                recipeInputLiquid = null;
            }
            else
            {
                recipeInputLiquid =
                    this.ViewModelManufacturingState.SelectedRecipe?.InputItems[0].ProtoItem as IProtoItemLiquidStorage;
            }

            var liquidType = recipeInputLiquid?.LiquidType
                             ?? this.privateState.LiquidType;

            (this.LiquidColor, this.LiquidIcon) = LiquidColorIconHelper.GetColorAndIcon(liquidType);
            this.LiquidTitle = liquidType?.GetDescription();
        }

        private void RefreshRecipe()
        {
            var recipe = this.ViewModelManufacturingState.SelectedRecipe;
            this.IsProgressBarInverted = recipe is IRecipeBarrelAddLiquid;
            this.ProgressBarVisibility =
                recipe != null
                && this.ViewModelManufacturingState.ManufacturingState.HasActiveRecipe
                    ? Visibility.Visible
                    : Visibility.Hidden;

            this.RefreshLiquidType();
        }
    }
}