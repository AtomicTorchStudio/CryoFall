namespace AtomicTorch.CBND.CoreMod.Editor.Controls.RecipeBreakdown
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class BootstrapperRecipeBreakdownInjector : BaseBootstrapper
    {
        private static readonly Dictionary<Control, Control> InjectedControls
            = new Dictionary<Control, Control>();

        public override void ClientInitialize()
        {
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
            var windowWrapper = VisualTreeHelperExtension.FindParentOfType(control, typeof(BaseUserControlWithWindow));
            var window = ((BaseUserControlWithWindow)windowWrapper).Window;

            var controlRecipeBreakdown = new ControlRecipeBreakdown();
            controlRecipeBreakdown.InheritedViewModel = viewModel;

            InjectedControls.Add(control, controlRecipeBreakdown);
            window.AddExtensionControl(controlRecipeBreakdown);
        }

        private static void RemoveControl(Control control)
        {
            if (!InjectedControls.TryGetValue(control, out var canvas))
            {
                return;
            }

            ((Panel)canvas.Parent).Children.Remove(canvas);
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