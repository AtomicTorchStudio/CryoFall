namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data;

    public partial class WindowTechnologies : BaseWindowMenu
    {
        private DialogWindow dialogWindowTechTreeChanged;

        private ViewModelWindowTechnologies viewModel;

        protected override void DisposeMenu()
        {
            base.DisposeMenu();
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        protected override void InitMenu()
        {
            this.DataContext = this.viewModel = new ViewModelWindowTechnologies();
        }

        protected override void WindowClosed()
        {
            base.WindowClosed();
            this.viewModel.ListSelectedTechGroup = null;
        }

        protected override void WindowClosing()
        {
            base.WindowClosing();

            this.dialogWindowTechTreeChanged?.Close(DialogResult.Cancel);
            this.dialogWindowTechTreeChanged = null;
        }

        protected override void WindowOpened()
        {
            base.WindowOpened();

            if (!ClientCurrentCharacterHelper.PrivateState.Technologies.IsTechTreeChanged)
            {
                return;
            }

            // the tech tree is changed, show a dialog window
            // please note that the "IsTechTreeChanged" is auto-reset when player research any technology
            this.dialogWindowTechTreeChanged = DialogWindow.ShowDialog(
                title: CoreStrings.TitleAttention,
                content: DialogWindow.CreateTextElement(
                    CoreStrings.WindowTechnologies_TechTreeChanged,
                    TextAlignment.Left),
                closeByEscapeKey: false);

            this.dialogWindowTechTreeChanged.Window.FocusOnControl = null;
        }
    }
}