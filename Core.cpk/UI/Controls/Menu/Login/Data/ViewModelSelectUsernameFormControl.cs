namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Login.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelSelectUsernameFormControl : BaseViewModel
    {
        private string username;

        public ViewModelSelectUsernameFormControl()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.CommandQuit = new ActionCommand(this.ExecuteCommandQuit);
            this.CommandContinue = new ActionCommand(this.ExecuteCommandContinue);

            if (Client.SteamApi.IsSteamClient)
            {
                this.Username = Client.SteamApi.SteamUsername;
            }
        }

        public BaseCommand CommandContinue { get; }

        public BaseCommand CommandQuit { get; }

        public string Username
        {
            get => this.username;
            set
            {
                if (this.username == value)
                {
                    return;
                }

                this.username = value;
                this.NotifyThisPropertyChanged();
            }
        }

        private void ExecuteCommandContinue()
        {
            try
            {
                Client.MasterServer.SelectUsername(this.Username);
            }
            catch (Exception ex)
            {
                DialogWindow.ShowDialog(title: null,
                                        ex.Message,
                                        zIndexOffset: 9002);
            }
        }

        private void ExecuteCommandQuit()
        {
            Client.Core.Quit();
        }
    }
}