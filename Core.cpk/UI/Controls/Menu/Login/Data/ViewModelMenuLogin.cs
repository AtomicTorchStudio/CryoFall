namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Login.Data
{
    using System;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelMenuLogin : BaseViewModel
    {
        public BaseCommand CommandQuit
            => new ActionCommand(() => Client.Core.Quit());

        public Visibility VisibilityConnectingControl { get; private set; } = Visibility.Collapsed;

        public Visibility VisibilityLoginAtomicTorchAccountForm { get; private set; } = Visibility.Collapsed;

        public Visibility VisibilitySelectUsernameControl { get; private set; } = Visibility.Collapsed;

        public Visibility VisibilitySteamAccountLinking { get; private set; } = Visibility.Collapsed;

        public Visibility VisibilitySteamError { get; private set; } = Visibility.Collapsed;

        public void Setup(MenuLoginMode mode)
        {
            Visibility visibilityLoginControl = Visibility.Collapsed,
                       visibilitySelectUsernameControl = Visibility.Collapsed,
                       visibilitySteamAccountLinking = Visibility.Collapsed,
                       visibilityConnectingControl = Visibility.Collapsed,
                       visibilitySteamError = Visibility.Collapsed;

            if (Api.Client.SteamApi.IsSteamClient
                && Api.Client.SteamApi.State == SteamApiState.Error)
            {
                mode = MenuLoginMode.SteamError;
            }

            switch (mode)
            {
                case MenuLoginMode.None:
                    // should be impossible as in this case the login menu is hidden
                    break;

                case MenuLoginMode.Connecting:
                    visibilityConnectingControl = Visibility.Visible;
                    break;

                case MenuLoginMode.SelectUsername:
                    visibilitySelectUsernameControl = Visibility.Visible;
                    break;

                case MenuLoginMode.SteamError:
                    visibilitySteamError = Visibility.Visible;
                    break;

                case MenuLoginMode.Login:
                    if (Api.Client.SteamApi.IsSteamClient)
                    {
                        visibilitySteamAccountLinking = Visibility.Visible;
                    }
                    else
                    {
                        visibilityLoginControl = Visibility.Visible;
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            this.VisibilityLoginAtomicTorchAccountForm = visibilityLoginControl;
            this.VisibilitySteamAccountLinking = visibilitySteamAccountLinking;
            this.VisibilitySelectUsernameControl = visibilitySelectUsernameControl;
            this.VisibilityConnectingControl = visibilityConnectingControl;
            this.VisibilitySteamError = visibilitySteamError;
        }
    }
}