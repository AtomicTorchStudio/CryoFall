namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Account
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Account.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public partial class WindowExternalAccountLinking : BaseUserControlWithWindow
    {
        private ViewModelExternalAccountLinkingWelcome viewModel;

        protected override void OnLoaded()
        {
            if (!Api.Client.ExternalApi.IsExternalClient)
            {
                throw new Exception("Not a Steam/Epic/external version of the game");
            }

            if (Api.Client.ExternalApi.IsLinkedAccount)
            {
                throw new Exception("External account is already linked");
            }

            this.DataContext = this.viewModel = new ViewModelExternalAccountLinkingWelcome(
                                   callbackResizeWindow: () =>
                                                         {
                                                             this.Window.Height = double.NaN;
                                                             this.Window.RefreshWindowSize();
                                                         },
                                   callbackClose: () => this.CloseWindow());

            Api.Client.MasterServer.LoggedInStateChanged += this.MasterServerLoggedInStateChangedHandler;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;

            Api.Client.MasterServer.LoggedInStateChanged -= this.MasterServerLoggedInStateChangedHandler;
        }

        private void MasterServerLoggedInStateChangedHandler()
        {
            this.CloseWindow();
        }
    }
}