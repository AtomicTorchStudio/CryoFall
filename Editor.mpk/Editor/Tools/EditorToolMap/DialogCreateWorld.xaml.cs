namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolMap
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    /// <summary>
    /// Interaction logic for DialogCreateWorld.xaml
    /// </summary>
    public partial class DialogCreateWorld : BaseUserControl
    {
        public Action OkCallback;

        public ViewModelDialogCreateWorld ViewModel { get; private set; }

        protected override void InitControl()
        {
            var dialogWindow = this.GetByName<DialogWindow>("DialogWindow");
            // to properly assign ZIndex of the window in the WindowsManager
            dialogWindow.Window.LinkedParent = this;
            dialogWindow.ForceShowCancelButton();
            dialogWindow.OkAction = this.InvokeOkCallback;

            this.DataContext = this.ViewModel = new ViewModelDialogCreateWorld();
        }

        private void InvokeOkCallback()
        {
            this.OkCallback?.Invoke();
        }
    }
}