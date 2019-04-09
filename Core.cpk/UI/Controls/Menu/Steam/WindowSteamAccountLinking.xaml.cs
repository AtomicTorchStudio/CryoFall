namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Steam
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Steam.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public partial class WindowSteamAccountLinking : BaseUserControlWithWindow
    {
        private ViewModelSteamAccountLinkingWelcome viewModel;

        protected override void OnLoaded()
        {
            if (!Api.Client.SteamApi.IsSteamClient)
            {
                throw new Exception("Not a Steam version of the game");
            }

            if (Api.Client.SteamApi.IsLinkedAccount)
            {
                throw new Exception("Steam account is already linked");
            }

            this.DataContext = this.viewModel = new ViewModelSteamAccountLinkingWelcome(
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