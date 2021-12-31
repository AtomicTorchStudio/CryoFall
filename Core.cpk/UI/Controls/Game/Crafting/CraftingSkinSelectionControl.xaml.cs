namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CraftingSkinSelectionControl : BaseUserControl
    {
        public static readonly DependencyProperty SelectedRecipeDetailsProperty
            = DependencyProperty.Register(nameof(SelectedRecipeDetails),
                                          typeof(ViewModelCraftingMenuRecipeDetails),
                                          typeof(CraftingSkinSelectionControl),
                                          new PropertyMetadata(null, SelectedRecipeDetailsPropertyChangedHandler));

        private Grid layoutRoot;

        private ViewModelCraftingSkinSelectionControl viewModel;

        public ViewModelCraftingMenuRecipeDetails SelectedRecipeDetails
        {
            get => (ViewModelCraftingMenuRecipeDetails)this.GetValue(SelectedRecipeDetailsProperty);
            set => this.SetValue(SelectedRecipeDetailsProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.layoutRoot.DataContext
                = this.viewModel
                      = new ViewModelCraftingSkinSelectionControl();
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            this.layoutRoot.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        private static void SelectedRecipeDetailsPropertyChangedHandler(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            ((CraftingSkinSelectionControl)d).Refresh();
        }

        private void Refresh()
        {
            if (this.isLoaded)
            {
                this.viewModel.SelectedRecipeDetails = this.SelectedRecipeDetails;
            }
        }
    }
}