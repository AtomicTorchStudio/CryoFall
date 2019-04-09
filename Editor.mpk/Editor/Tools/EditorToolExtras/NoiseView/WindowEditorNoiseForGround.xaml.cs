namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolExtras.NoiseView
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolExtras.NoiseView.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public partial class WindowEditorNoiseForGround : BaseUserControlWithWindow
    {
        private ClientInputContext suppressInputContext;

        private ViewModelWindowEditorNoiseForGround viewModel;

        public WindowEditorNoiseForGround()
        {
        }

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
                                        .Start("Window noise - intercept all other input")
                                        .HandleAll(ClientInputManager.ConsumeAllButtons);

            this.DataContext = this.viewModel = new ViewModelWindowEditorNoiseForGround();
        }
    }
}