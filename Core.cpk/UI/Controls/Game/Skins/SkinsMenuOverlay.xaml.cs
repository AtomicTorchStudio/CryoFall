namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skins
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class SkinsMenuOverlay : BaseUserControl
    {
        public static readonly DependencyProperty CommandCloseProperty
            = DependencyProperty.Register("CommandClose",
                                          typeof(BaseCommand),
                                          typeof(SkinsMenuOverlay),
                                          new PropertyMetadata(default(BaseCommand)));

        private static SkinsMenuOverlay instance;

        private static bool isDisplayed;

        private static ClientInputContext openedMenuInputContext;

        public static bool IsDisplayed
        {
            get => isDisplayed;
            set
            {
                if (isDisplayed == value)
                {
                    return;
                }

                isDisplayed = value;

                if (isDisplayed)
                {
                    // display menu
                    if (instance is null)
                    {
                        instance = new SkinsMenuOverlay();
                        Api.Client.UI.LayoutRootChildren.Add(instance);
                    }

                    instance.Visibility = Visibility.Visible;
                    OnBecomeVisible();
                }
                else
                {
                    // hide menu
                    if (instance is null)
                    {
                        return;
                    }

                    OnBecomeHidden();
                }
            }
        }

        public BaseCommand CommandClose
        {
            get => (BaseCommand)this.GetValue(CommandCloseProperty);
            set => this.SetValue(CommandCloseProperty, value);
        }

        protected override void InitControl()
        {
            this.CommandClose = new ActionCommand(() => IsDisplayed = false);
        }

        private static void OnBecomeHidden()
        {
            openedMenuInputContext?.Stop();
            openedMenuInputContext = null;
            VisualStateManager.GoToElementState(instance, "Hidden", true);
            ClientTimersSystem.AddAction(0.333,
                                         () =>
                                         {
                                             if (!IsDisplayed)
                                             {
                                                 instance.Visibility = Visibility.Collapsed;
                                                 BootstrapperClientCore.DisconnectMasterServerIfNotNecessary();
                                             }
                                         });
        }

        private static void OnBecomeVisible()
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            openedMenuInputContext = ClientInputContext.Start("Skins menu - intercept all other input")
                                                       .HandleAll(() =>
                                                                  {
                                                                      if (ClientInputManager.IsButtonDown(
                                                                          GameButton.CancelOrClose))
                                                                      {
                                                                          IsDisplayed = false;
                                                                      }

                                                                      ClientInputManager.ConsumeAllButtons();
                                                                  });
            VisualStateManager.GoToElementState(instance, "Displayed", true);
        }
    }
}