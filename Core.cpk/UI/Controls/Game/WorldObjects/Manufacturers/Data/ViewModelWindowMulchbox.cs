namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelWindowMulchbox : ViewModelWindowManufacturer
    {
        private readonly ObjectMulchboxPrivateState privateState;

        public ViewModelWindowMulchbox()
        {
        }

        public ViewModelWindowMulchbox(
            IStaticWorldObject worldObjectManufacturer,
            ManufacturingState manufacturingState,
            ManufacturingConfig manufacturingConfig)
            : base(
                worldObjectManufacturer,
                manufacturingState,
                manufacturingConfig,
                null)
        {
            var protoMulchbox = (IProtoObjectMulchbox)worldObjectManufacturer.ProtoStaticWorldObject;
            this.OrganicCapacity = protoMulchbox.OrganicCapacity;

            this.privateState = protoMulchbox.GetMulchboxPrivateState(worldObjectManufacturer);
            this.privateState.ClientSubscribe(
                _ => _.OrganicAmount,
                _ => this.RefreshOrganicAmount(),
                this);

            manufacturingState.ClientSubscribe(
                _ => _.SelectedRecipe,
                _ => this.RefreshRecipe(),
                this);

            this.RefreshOrganicAmount();
            this.RefreshRecipe();
        }

        public ushort OrganicAmount { get; set; }

        public ushort OrganicCapacity { get; }

        public Visibility ProgressBarVisibility { get; set; } = Visibility.Visible;

        private void RefreshOrganicAmount()
        {
            this.OrganicAmount = this.privateState.OrganicAmount;
        }

        private void RefreshRecipe()
        {
            var recipe = this.ViewModelManufacturingState.SelectedRecipe;
            this.ProgressBarVisibility = recipe != null ? Visibility.Visible : Visibility.Hidden;
        }
    }
}