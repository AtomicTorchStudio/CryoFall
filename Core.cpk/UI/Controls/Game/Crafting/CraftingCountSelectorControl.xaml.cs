namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting
{
    using System.Windows.Controls;
    using System.Windows.Input;
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

            if (key == Key.Tab)
            {
                return;
            }

            if (key == Key.Escape)
            {
                Api.Client.UI.BlurFocus();
                return;
            }

            // suppress key input
            e.Handled = true;
        }
    }
}