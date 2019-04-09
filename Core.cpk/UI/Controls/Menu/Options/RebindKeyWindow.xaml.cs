namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options.Data;

    public partial class RebindKeyWindow : BaseUserControlWithWindow
    {
        private bool isSecondaryKey;

        private ViewModelButtonMappingControl viewModelMapping;

        public void Setup(ViewModelButtonMappingControl viewModelMapping, bool isSecondaryKey)
        {
            this.viewModelMapping = viewModelMapping;
            this.isSecondaryKey = isSecondaryKey;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            this.DataContext = new ViewModelRebindKeyWindow(
                this.viewModelMapping,
                this.isSecondaryKey,
                onClose: () => this.CloseWindow(DialogResult.Cancel));
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            ((IDisposable)this.DataContext).Dispose();
            this.DataContext = null;
        }
    }
}