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

        public Visibility VisibilityEpicLauncherError { get; private set; } = Visibility.Collapsed;

        public Visibility VisibilityExternalAccountLinking { get; private set; } = Visibility.Collapsed;

        public Visibility VisibilityLoginAtomicTorchAccountForm { get; private set; } = Visibility.Collapsed;

        public Visibility VisibilitySelectUsernameControl { get; private set; } = Visibility.Collapsed;

        public Visibility VisibilitySteamError { get; private set; } = Visibility.Collapsed;

        public void Setup(MenuLoginMode mode)
        {
            Visibility visibilityLoginControl = Visibility.Collapsed,
                       visibilitySelectUsernameControl = Visibility.Collapsed,
                       visibilityExternalAccountLinking = Visibility.Collapsed,
                       visibilityConnectingControl = Visibility.Collapsed,
                       visibilitySteamError = Visibility.Collapsed,
                       visibilityEpicLauncherError = Visibility.Collapsed;

            var externalApi = Api.Client.ExternalApi;
            if (externalApi.IsExternalClient
                && externalApi.State == ExternalApiState.Error)
            {
                if (externalApi.IsSteamClient)
                {
                    mode = MenuLoginMode.SteamError;
                }
                else if (externalApi.IsEpicClient)
                {
                    mode = MenuLoginMode.EpicLauncherError;
                }
                else
                {
                    throw new Exception("Unknown external client");
                }
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

                case MenuLoginMode.EpicLauncherError:
                    visibilityEpicLauncherError = Visibility.Visible;
                    break;

                case MenuLoginMode.Login:
                    if (externalApi.IsExternalClient)
                    {
                        visibilityExternalAccountLinking = Visibility.Visible;
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
            this.VisibilityExternalAccountLinking = visibilityExternalAccountLinking;
            this.VisibilitySelectUsernameControl = visibilitySelectUsernameControl;
            this.VisibilityConnectingControl = visibilityConnectingControl;
            this.VisibilitySteamError = visibilitySteamError;
            this.VisibilityEpicLauncherError = visibilityEpicLauncherError;
        }
    }
}