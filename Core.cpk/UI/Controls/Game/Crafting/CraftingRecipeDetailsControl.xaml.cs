namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CraftingRecipeDetailsControl : BaseUserControl
    {
        public static readonly DependencyProperty ViewModelRecipeProperty =
            DependencyProperty.Register(
                nameof(ViewModelRecipe),
                typeof(ViewModelCraftingRecipe),
                typeof(CraftingRecipeDetailsControl),
                new PropertyMetadata(null, RecipeChangedHandler));

        public static readonly DependencyProperty ViewModelRecipeDetailsProperty
            = DependencyProperty.Register("ViewModelRecipeDetails",
                                          typeof(ViewModelCraftingMenuRecipeDetails),
                                          typeof(CraftingRecipeDetailsControl),
                                          new PropertyMetadata(null, RecipeChangedHandler));

        private bool isOwnedViewModel;

        private Grid layoutRoot;

        private ViewModelCraftingMenuRecipeDetails viewModel;

        public static event Action<CraftingRecipeDetailsControl, IViewModelWithRecipe> ControlLoaded;

        public static event Action<CraftingRecipeDetailsControl> ControlUnloaded;

        public ViewModelCraftingRecipe ViewModelRecipe
        {
            get => (ViewModelCraftingRecipe)this.GetValue(ViewModelRecipeProperty);
            set => this.SetValue(ViewModelRecipeProperty, value);
        }

        public ViewModelCraftingMenuRecipeDetails ViewModelRecipeDetails
        {
            get => (ViewModelCraftingMenuRecipeDetails)this.GetValue(ViewModelRecipeDetailsProperty);
            set => this.SetValue(ViewModelRecipeDetailsProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.RefreshViewModel();
        }

        protected override void OnUnloaded()
        {
            if (this.ViewModelRecipeDetails is not null)
            {
                Api.SafeInvoke(() => ControlUnloaded?.Invoke(this));
            }

            this.layoutRoot.DataContext = null;
            this.DestroyOwnedViewModel();
        }

        private static void RecipeChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CraftingRecipeDetailsControl)d;
            control.RefreshViewModel();
        }

        private void DestroyOwnedViewModel()
        {
            if (!this.isOwnedViewModel)
            {
                return;
            }

            this.layoutRoot.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
            this.isOwnedViewModel = false;
            Api.SafeInvoke(() => ControlUnloaded?.Invoke(this));
        }

        private void RefreshViewModel()
        {
            if (!this.isLoaded)
            {
                return;
            }
            
            if (this.ViewModelRecipeDetails is not null)
            {
                this.DestroyOwnedViewModel();
                
                if (this.layoutRoot.DataContext is not null)
                {
                    Api.SafeInvoke(() => ControlUnloaded?.Invoke(this));
                }

                this.layoutRoot.DataContext = this.ViewModelRecipeDetails;
                Api.SafeInvoke(() => ControlLoaded?.Invoke(this, this.ViewModelRecipeDetails));
                return;
            }

            if (this.ViewModelRecipe is not null)
            {
                if (this.viewModel is null)
                {
                    this.viewModel = new ViewModelCraftingMenuRecipeDetails(
                        validateItemsAvailabilityInPlayerInventory: true);
                    this.isOwnedViewModel = true;
                    this.layoutRoot.DataContext = this.viewModel;
                    Api.SafeInvoke(() => ControlLoaded?.Invoke(this, this.viewModel));
                }

                this.viewModel.ViewModelRecipe = this.ViewModelRecipe;
            }
        }
    }
}