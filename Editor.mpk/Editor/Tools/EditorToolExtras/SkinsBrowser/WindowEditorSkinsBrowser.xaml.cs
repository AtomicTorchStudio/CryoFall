namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolExtras.SkinsBrowser
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolExtras.SkinsBrowser.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public partial class WindowEditorSkinsBrowser : BaseUserControlWithWindow
    {
        private ClientInputContext suppressInputContext;

        private ViewModelWindowEditorSkinsBrowser viewModel;

        protected override void WindowClosing()
        {
            this.suppressInputContext.Stop();
            this.suppressInputContext = null;

            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        protected override void WindowOpening()
        {
            this.suppressInputContext = ClientInputContext
                                        .Start("Window items browser - intercept all other input")
                                        .HandleAll(() =>
                                                   {
                                                       if (ClientInputManager.IsButtonDown(GameButton.CancelOrClose))
                                                       {
                                                           this.CloseWindow();
                                                       }

                                                       ClientInputManager.ConsumeAllButtons();
                                                   });

            this.DataContext = this.viewModel = new ViewModelWindowEditorSkinsBrowser(
                                   closeCallback: () => this.CloseWindow());
        }
    }
}