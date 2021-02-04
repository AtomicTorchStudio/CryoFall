namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu
{
    using System;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using JetBrains.Annotations;

    public partial class MainMenuOverlay : BaseUserControl
    {
        private static MainMenuOverlay instance;

        private static bool isHidden = true;

        private static ClientInputContext openedMainMenuInputContext;

        public static event Action IsHiddenChanged;

        public static bool IsHidden
        {
            get => isHidden;
            set
            {
                if (isHidden == value)
                {
                    return;
                }

                if (value && instance is not null)
                {
                    // will hide - ensure the active tab allows to be hidden
                    // (options tab might not allow that if there are unapplied changes)
                    if (!instance.Options.CheckCanHide(() => IsHidden = true))
                    {
                        return;
                    }
                }

                isHidden = value;

                Api.Logger.Info("MenuOverlay is " + (isHidden ? "hidden" : "shown"));
                Api.Client.UI.BlurFocus();

                if (isHidden)
                {
                    if (instance is null)
                    {
                        // instance is not yet created!
                        return;
                    }

                    instance.Visibility = Visibility.Collapsed;
                    OnBecomeHidden();
                }
                else
                {
                    // display menu
                    if (instance is null)
                    {
                        instance = new MainMenuOverlay();
                        Api.Client.UI.LayoutRootChildren.Add(instance);
                    }

                    instance.Visibility = Visibility.Visible;
                    OnBecomeVisible();
                }

                IsHiddenChanged?.Invoke();
            }
        }

        public static bool IsOptionsMenuSelected
            => !IsHidden
               && ViewModelMainMenuOverlay.Instance.IsOptionsMenuSelected;

        public MenuOptions Options { get; private set; }

        public static void DestroyIfPresent()
        {
            if (instance is null)
            {
                return;
            }

            IsHidden = true;
            Api.Client.UI.LayoutRootChildren.Remove(instance);
            instance = null;
        }

        [CanBeNull]
        public static MenuOptions GetOptions()
        {
            return instance?.Options;
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

        protected override void InitControl()
        {
            this.Options = this.GetByName<MenuOptions>("MenuOptions");
            this.DataContext = ViewModelMainMenuOverlay.Instance;
        }

        private static void OnBecomeHidden()
        {
            openedMainMenuInputContext?.Stop();
            openedMainMenuInputContext = null;
            ServerViewModelsProvider.Instance.IsEnabled = false;

            BootstrapperClientCore.DisconnectMasterServerIfNotNecessary();
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