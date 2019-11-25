namespace AtomicTorch.CBND.CoreMod.Editor.Controls.RecipeBreakdown
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class BootstrapperRecipeBreakdownInjector : BaseBootstrapper
    {
        private static readonly Dictionary<Control, Canvas> InjectedControls
            = new Dictionary<Control, Canvas>();

        public override void ClientInitialize()
        {
            base.ClientInitialize();
            CraftingRecipeDetailsControl.ControlLoaded += CraftingRecipeDetailsControlLoadedHandler;
            CraftingRecipeDetailsControl.ControlUnloaded += CraftingRecipeDetailsControlUnloadedHandler;

            RecipesBrowserRecipeDetailsControl.ControlLoaded += this.RecipesBrowserRecipeDetailsControlLoadedHandler;
            RecipesBrowserRecipeDetailsControl.ControlUnloaded +=
                this.RecipesBrowserRecipeDetailsControlUnloadedHandler;
        }

        private static void CraftingRecipeDetailsControlLoadedHandler(
            CraftingRecipeDetailsControl control,
            IViewModelWithRecipe viewModel)
        {
            InjectControl(control, viewModel);
        }

        private static void CraftingRecipeDetailsControlUnloadedHandler(CraftingRecipeDetailsControl control)
        {
            RemoveControl(control);
        }

        private static void InjectControl(Control control, IViewModelWithRecipe viewModel)
        {
            var window = VisualTreeHelperExtension.FindParentOfType(control, typeof(BaseUserControlWithWindow));

            var firstChild = (FrameworkElement)VisualTreeHelper.GetChild(
                ((BaseUserControlWithWindow)window).Window,
                0);
            var grid = firstChild.FindName<Grid>("ContentChromeGrid");

            var canvas = new Canvas
            {
                HorizontalAlignment = HorizontalAlignment.Right,
            };

            var controlRecipeBreakdown = new ControlRecipeBreakdown();
            controlRecipeBreakdown.InheritedViewModel = viewModel;
            canvas.Children.Add(controlRecipeBreakdown);
            Canvas.SetLeft(controlRecipeBreakdown, 10);

            grid.Children.Add(canvas);
            InjectedControls.Add(control, canvas);
        }

        private static void RemoveControl(Control control)
        {
            if (!InjectedControls.TryGetValue(control, out var canvas))
            {
                return;
            }

            ((Grid)canvas.Parent).Children.Remove(canvas);
            InjectedControls.Remove(control);
        }

        private void RecipesBrowserRecipeDetailsControlLoadedHandler(
            RecipesBrowserRecipeDetailsControl control,
            IViewModelWithRecipe viewModel)
        {
            InjectControl(control, viewModel);
        }

        private void RecipesBrowserRecipeDetailsControlUnloadedHandler(RecipesBrowserRecipeDetailsControl control)
        {
            RemoveControl(control);
        }
    }
}