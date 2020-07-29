namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class RecipeTooltip : BaseUserControl
    {
        public static readonly DependencyProperty RecipeProperty = DependencyProperty.Register(
            nameof(Recipe),
            typeof(ViewModelCraftingRecipe),
            typeof(RecipeTooltip),
            new PropertyMetadata(default(ViewModelCraftingRecipe), PropertyChangedCallback));

        private FrameworkElement layoutRoot;

        private ViewModelRecipesBrowserRecipeDetails viewModel;

        public ViewModelCraftingRecipe Recipe
        {
            get => (ViewModelCraftingRecipe)this.GetValue(RecipeProperty);
            set => this.SetValue(RecipeProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Viewbox>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            this.Refresh();
        }

        private static void PropertyChangedCallback(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((RecipeTooltip)dependencyObject).Refresh();
        }

        private void Refresh()
        {
            if (!this.isLoaded)
            {
                if (this.viewModel == null)
                {
                    return;
                }

                this.layoutRoot.DataContext = null;
                this.viewModel.Dispose();
                this.viewModel = null;
                return;
            }

            this.viewModel ??= new ViewModelRecipesBrowserRecipeDetails(null, null);
            this.viewModel.ViewModelRecipe = this.Recipe;
            this.layoutRoot.DataContext = this.viewModel;
        }
    }
}