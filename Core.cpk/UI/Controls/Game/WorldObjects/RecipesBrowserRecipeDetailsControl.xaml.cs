namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class RecipesBrowserRecipeDetailsControl : BaseUserControl
    {
        public static readonly DependencyProperty RecipeProperty =
            DependencyProperty.Register(
                nameof(Recipe),
                typeof(ViewModelCraftingRecipe),
                typeof(RecipesBrowserRecipeDetailsControl),
                new PropertyMetadata(null, RecipeChangedHandler));

        public static readonly DependencyProperty CommandCancelProperty =
            DependencyProperty.Register(
                nameof(CommandCancel),
                typeof(BaseCommand),
                typeof(RecipesBrowserRecipeDetailsControl),
                new PropertyMetadata(default(BaseCommand)));

        public static readonly DependencyProperty CommandRecipeSelectedProperty =
            DependencyProperty.Register(
                nameof(CommandRecipeSelected),
                typeof(BaseCommand),
                typeof(RecipesBrowserRecipeDetailsControl),
                new PropertyMetadata(default(BaseCommand)));

        private Grid layoutRoot;

        public static event Action<RecipesBrowserRecipeDetailsControl, IViewModelWithRecipe> ControlLoaded;

        public static event Action<RecipesBrowserRecipeDetailsControl> ControlUnloaded;

        public BaseCommand CommandCancel
        {
            get => (BaseCommand)this.GetValue(CommandCancelProperty);
            set => this.SetValue(CommandCancelProperty, value);
        }

        public BaseCommand CommandRecipeSelected
        {
            get => (BaseCommand)this.GetValue(CommandRecipeSelectedProperty);
            set => this.SetValue(CommandRecipeSelectedProperty, value);
        }

        public ViewModelCraftingRecipe Recipe
        {
            get => (ViewModelCraftingRecipe)this.GetValue(RecipeProperty);
            set => this.SetValue(RecipeProperty, value);
        }

        public ViewModelRecipesBrowserRecipeDetails ViewModel { get; private set; }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.ViewModel = new ViewModelRecipesBrowserRecipeDetails(
                onSelected:
                this.CommandRecipeSelected == null
                    ? (Action)null
                    : () => this.CommandRecipeSelected.Execute(this.Recipe.Recipe),
                onCancel: this.CommandCancel == null ? (Action)null : () => this.CommandCancel.Execute(null));

            this.layoutRoot.DataContext = this.ViewModel;

            if (IsDesignTime)
            {
                return;
            }

            this.RefreshViewModel();
            Api.SafeInvoke(() => ControlLoaded?.Invoke(this, this.ViewModel));
        }

        protected override void OnUnloaded()
        {
            Api.SafeInvoke(() => ControlUnloaded?.Invoke(this));

            this.DataContext = null;
            this.ViewModel.Dispose();
            this.ViewModel = null;
        }

        private static void RecipeChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (RecipesBrowserRecipeDetailsControl)d;
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
            this.ViewModel.ViewModelRecipe = this.Recipe;
        }
    }
}