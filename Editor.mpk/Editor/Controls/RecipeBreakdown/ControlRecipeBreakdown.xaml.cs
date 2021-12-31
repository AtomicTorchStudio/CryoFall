namespace AtomicTorch.CBND.CoreMod.Editor.Controls.RecipeBreakdown
{
    using AtomicTorch.CBND.CoreMod.Editor.Controls.RecipeBreakdown.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ControlRecipeBreakdown : BaseUserControl
    {
        private ViewModelControlRecipeBreakdown viewModel;

        public IViewModelWithRecipe InheritedViewModel { get; set; }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelControlRecipeBreakdown(this.InheritedViewModel);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.InheritedViewModel = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}