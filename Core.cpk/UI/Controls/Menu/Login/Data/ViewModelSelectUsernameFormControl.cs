namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Login.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelSelectUsernameFormControl : BaseViewModel
    {
        // {0} is the selected username. We want to be sure the player is selected a good username.
        public const string Dialog_AcceptUsername =
            @"Is [b]{0}[/b] okay? You will not be able to change it later.
              [br]This name will be used in-game when you play on any game server.";

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
            DialogWindow.ShowDialog(
                title: CoreStrings.QuestionAreYouSure,
                string.Format(Dialog_AcceptUsername, this.username),
                okText: CoreStrings.Button_Accept,
                okAction: () =>
                          {
                              try
                              {
                                  Client.MasterServer.SelectUsername(this.username);
                              }
                              catch (Exception ex)
                              {
                                  DialogWindow.ShowDialog(title: null,
                                                          ex.Message,
                                                          zIndexOffset: 9002);
                              }
                          },
                cancelAction: () => { },
                focusOnCancelButton: true);
        }

        private void ExecuteCommandQuit()
        {
            Client.Core.Quit();
        }
    }
}