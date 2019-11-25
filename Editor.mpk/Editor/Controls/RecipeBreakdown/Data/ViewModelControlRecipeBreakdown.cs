namespace AtomicTorch.CBND.CoreMod.Editor.Controls.RecipeBreakdown.Data
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;

    public class ViewModelControlRecipeBreakdown : BaseViewModel
    {
        private IViewModelWithRecipe inheritedViewModel;

        public ViewModelControlRecipeBreakdown(IViewModelWithRecipe inheritedViewModel)
        {
            this.InheritedViewModel = inheritedViewModel;

            ((BaseViewModel)this.inheritedViewModel).PropertyChanged
                += this.InheritedViewModelOnPropertyChangedHandler;

            this.RefreshRecipe();
        }

        public ViewModelControlRecipeBreakdown()
        {
        }

        // ReSharper disable once ConvertToAutoProperty
        [ViewModelNotAutoDisposeField]
        public IViewModelWithRecipe InheritedViewModel
        {
            get => this.inheritedViewModel;
            set => this.inheritedViewModel = value;
        }

        public ViewModelRecipeBreakdown ViewModelRecipeBreakdown { get; private set; }

        protected override void DisposeViewModel()
        {
            if (this.inheritedViewModel != null)
            {
                ((BaseViewModel)this.inheritedViewModel).PropertyChanged
                    -= this.InheritedViewModelOnPropertyChangedHandler;
            }

            base.DisposeViewModel();
        }

        private void InheritedViewModelOnPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ViewModelRecipe")
            {
                this.RefreshRecipe();
            }
        }

        private void RefreshRecipe()
        {
            var oldViewModel = this.ViewModelRecipeBreakdown;
            if (oldViewModel != null)
            {
                this.ViewModelRecipeBreakdown = null;
                oldViewModel.Dispose();
            }

            var recipe = this.inheritedViewModel.ViewModelRecipe?.Recipe;
            if (recipe == null)
            {
                return;
            }

            this.ViewModelRecipeBreakdown = new ViewModelRecipeBreakdown(recipe);
        }
    }
}