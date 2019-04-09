namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ButtonMappingControl : BaseUserControl
    {
        private IWrappedButton button;

        private ButtonInfoAttribute buttonInfo;

        public void Setup(IWrappedButton button, ButtonInfoAttribute buttonInfo)
        {
            this.button = button;
            this.buttonInfo = buttonInfo;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            this.DataContext = new ViewModelButtonMappingControl(this.button, this.buttonInfo);
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            ((IDisposable)this.DataContext).Dispose();
            this.DataContext = null;
        }
    }
}