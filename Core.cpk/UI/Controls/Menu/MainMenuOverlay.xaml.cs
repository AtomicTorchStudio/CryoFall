namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu
{
    using System;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MainMenuOverlay : BaseUserControl
    {
        private static MainMenuOverlay instance;

        private static bool isHidden = true;

        private static ClientInputContext openedMainMenuInputContext;

        private ViewModelMainMenuOverlay viewModel;

        public static event Action IsHiddenChanged;

        public static MainMenuOverlay Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MainMenuOverlay();
                    Api.Client.UI.LayoutRootChildren.Add(instance);
                    UpdateActiveTab();
                }

                return instance;
            }
        }

        public static bool IsHidden
        {
            get => isHidden;
            set
            {
                if (isHidden == value)
                {
                    return;
                }

                if (value && instance != null)
                {
                    // will hide - ensure the active tab allows to be hidden
                    // (options tab might not allow that if there are unapplied changes)
                    if (!instance.viewModel.CheckCanHide(() => IsHidden = true))
                    {
                        return;
                    }
                }

                isHidden = value;

                Api.Logger.Info("MenuOverlay is " + (isHidden ? "hidden" : "shown"));
                Api.Client.UI.BlurFocus();

                if (isHidden)
                {
                    if (instance == null)
                    {
                        // instance is not yet created!
                        return;
                    }

                    Instance.Visibility = Visibility.Collapsed;
                    OnBecomeHidden();
                }
                else
                {
                    // display menu
                    Instance.Visibility = Visibility.Visible;
                    OnBecomeVisible();
                }

                IsHiddenChanged?.Invoke();
            }
        }

        public static bool IsOptionsMenuSelected
            => !IsHidden
               && (instance?.viewModel?.IsOptionsMenuSelected
                   ?? false);

        public static void DestroyIfPresent()
        {
            if (instance == null)
            {
                return;
            }

            IsHidden = true;
            Api.Client.UI.LayoutRootChildren.Remove(instance);
            instance = null;
        }

        public static void Toggle()
        {
            var currentGameService = Api.Client.CurrentGame;
            if (currentGameService.ConnectionState == ConnectionState.Connected
                || currentGameService.ConnectionState == ConnectionState.Connecting)
            {
                // allow toggling menu overlay when not disconnected from the server
                IsHidden = !IsHidden;
            }
            else
            {
                // not connected and not connecting! Force displaying menu overlay
                IsHidden = false;
            }
        }

        /// <summary>
        /// Selects "Current game" tab if main menu overlay instance is not null and game server is connected or connecting.
        /// </summary>
        public static void UpdateActiveTab()
        {
            if (instance == null)
            {
                return;
            }

            var connectionState = Api.Client.CurrentGame.ConnectionState;
            instance.viewModel.IsCurrentGameTabSelected = connectionState == ConnectionState.Connecting
                                                          || connectionState == ConnectionState.Connected;
        }

        protected override void InitControl()
        {
            var menuOptions = this.GetByName<MenuOptions>("MenuOptions");
            this.DataContext = this.viewModel = new ViewModelMainMenuOverlay(menuOptions);
        }

        private static void OnBecomeHidden()
        {
            openedMainMenuInputContext?.Stop();
            openedMainMenuInputContext = null;
            ServerViewModelsProvider.Instance.IsEnabled = false;
        }

        private static void OnBecomeVisible()
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            openedMainMenuInputContext =
                ClientInputContext.Start("Main menu - intercept all other input")
                                  .HandleAll(
                                      () =>
                                      {
                                          if (ClientInputManager.IsButtonDown(GameButton.CancelOrClose))
                                          {
                                              Toggle();
                                          }

                                          ClientInputManager.ConsumeAllButtons();
                                      });

            ServerViewModelsProvider.Instance.IsEnabled = true;
        }
    }
}