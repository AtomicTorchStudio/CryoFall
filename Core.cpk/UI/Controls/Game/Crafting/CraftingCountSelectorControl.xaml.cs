namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting
{
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CraftingCountSelectorControl : BaseUserControl
    {
        private TextBox textBoxCount;

        protected override void InitControl()
        {
            this.textBoxCount = this.GetByName<TextBox>("TextBoxCount");
        }

        protected override void OnLoaded()
        {
            this.textBoxCount.KeyDown += this.TextBoxCountKeyDownHandler;
        }

        protected override void OnUnloaded()
        {
            this.textBoxCount.KeyDown -= this.TextBoxCountKeyDownHandler;
        }

        // Do not allow printing anything except digits into the count textbox.
        private void TextBoxCountKeyDownHandler(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            var isDigit = key >= Key.D0 && key <= Key.D9
                          || key >= Key.NumPad0 && key <= Key.NumPad9;

            if (isDigit)
            {
                // allow input
                return;
            }

            switch (key)
            {
                case Key.Tab:
                case Key.Escape:
                    Api.Client.UI.BlurFocus();
                    return;

                case Key.Enter:
                    if (this.DataContext is ViewModelCraftingMenuRecipeDetails viewModel)
                    {
                        viewModel.CommandCraft.Execute(null);
                    }

                    return;

                default:
                    // suppress key input
                    e.Handled = true;
                    break;
            }
        }
    }
}