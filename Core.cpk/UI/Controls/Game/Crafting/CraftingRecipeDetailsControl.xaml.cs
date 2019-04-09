namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CraftingRecipeDetailsControl : BaseUserControl
    {
        // Using a DependencyProperty as the backing store for ViewModelRecipe.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelRecipeProperty =
            DependencyProperty.Register(
                nameof(ViewModelRecipe),
                typeof(ViewModelCraftingRecipe),
                typeof(CraftingRecipeDetailsControl),
                new PropertyMetadata(null, RecipeChangedHandler));

        private Grid layoutRoot;

        public ViewModelCraftingMenuRecipeDetails ViewModel { get; private set; }

        public ViewModelCraftingRecipe ViewModelRecipe
        {
            get => (ViewModelCraftingRecipe)this.GetValue(ViewModelRecipeProperty);
            set => this.SetValue(ViewModelRecipeProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.ViewModel = new ViewModelCraftingMenuRecipeDetails(validateItemsAvailabilityInPlayerInventory: true);
            this.layoutRoot.DataContext = this.ViewModel;

            if (IsDesignTime)
            {
                return;
            }

            this.RefreshViewModel();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.ViewModel.Dispose();
            this.ViewModel = null;
        }

        private static void RecipeChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CraftingRecipeDetailsControl)d;
            control.RefreshViewModel();
        }

        private void RefreshViewModel()
        {
            if (IsDesignTime
                || !this.IsLoaded)
            {
                return;
            }

            //Api.Logger.Write($"Recipe changed to {this.Recipe.Recipe}, updating view model for {this}");
            this.ViewModel.ViewModelRecipe = this.ViewModelRecipe;
        }
    }
}