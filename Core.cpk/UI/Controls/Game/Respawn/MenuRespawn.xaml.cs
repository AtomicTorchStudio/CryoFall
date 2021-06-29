namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Respawn
{
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Respawn.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuRespawn : BaseUserControl
    {
        /// <summary>
        /// When fade-in used (such as in the case of character death), a short delay is necessary.
        /// Its duration is defined by this constant.
        /// </summary>
        private const int FadeInDelaySeconds = 2;

        private static ClientInputContext inputContext;

        private static MenuRespawn instance;

        private static bool isClosed = true;

        private bool IsFadeIn;

        private bool isOpened;

        private Storyboard storyboardFadeIn;

        public static void EnsureClosed()
        {
            if (isClosed)
            {
                return;
            }

            isClosed = true;
            instance?.CloseMenu();
        }

        public static void EnsureOpened(bool fadeIn)
        {
            if (!isClosed)
            {
                return;
            }

            isClosed = false;

            if (fadeIn)
            {
                if (LoadingSplashScreenManager.Instance.CurrentState
                        is LoadingSplashScreenState.Shown
                        or LoadingSplashScreenState.Showing)
                {
                    // no fade-in required
                    fadeIn = false;
                }
            }

            var menu = new MenuRespawn();
            menu.IsFadeIn = fadeIn;
            instance = menu;

            Api.Client.UI.LayoutRootChildren.Add(instance);

            ClientTimersSystem.AddAction(
                delaySeconds: fadeIn ? FadeInDelaySeconds : 0,
                action: () =>
                        {
                            if (isClosed)
                            {
                                if (ReferenceEquals(instance, menu))
                                {
                                    instance = null;
                                    Api.Client.UI.LayoutRootChildren.Remove(instance);
                                }

                                return;
                            }

                            instance.Open();
                        });
        }

        protected override void InitControl()
        {
            this.DataContext = new ViewModelWindowRespawn(
                callbackRefreshHeght: () => { });

            // special hack for NoesisGUI animation completed event
            this.Tag = this;

            if (this.IsFadeIn)
            {
                this.storyboardFadeIn = AnimationHelper.CreateStoryboard(
                    this,
                    durationSeconds: 2,
                    from: 0,
                    to: 1,
                    propertyName: OpacityProperty.Name);
                this.Opacity = 0;
            }
        }

        private void BackgroundFadeOutCompleted()
        {
            Api.Client.UI.LayoutRootChildren.Remove(this);
            inputContext?.Stop();
            inputContext = null;
        }

        private void CloseMenu()
        {
            this.storyboardFadeIn?.Stop(this);

            AnimationHelper.CreateStoryboard(
                               this,
                               durationSeconds: 0.667,
                               from: this.Opacity,
                               to: 0,
                               propertyName: OpacityProperty.Name,
                               onCompleted: this.BackgroundFadeOutCompleted)
                           .Begin(this);

            if (instance == this)
            {
                instance = null;
            }
        }

        private void Open()
        {
            if (this.isOpened)
            {
                return;
            }

            this.isOpened = true;
            this.UpdateLayout();
            this.storyboardFadeIn?.Begin(this);

            Menu.CloseAll();

            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            inputContext = ClientInputContext.Start("Respawn menu - intercept all other input")
                                             .HandleAll(
                                                 () =>
                                                 {
                                                     if (ClientInputManager.IsButtonDown(GameButton.CancelOrClose))
                                                     {
                                                         MainMenuOverlay.Toggle();
                                                     }

                                                     ClientInputManager.ConsumeAllButtons();
                                                 });
        }
    }
}