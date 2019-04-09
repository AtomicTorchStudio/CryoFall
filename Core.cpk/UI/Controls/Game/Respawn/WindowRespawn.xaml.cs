namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Respawn
{
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Timer;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Respawn.Data;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.CoreMod.UI.Services;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using Menu = AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu.Menu;

    public partial class WindowRespawn : BaseUserControlWithWindow
    {
        private static WindowRespawn instance;

        private static bool isClosed = true;

        private Rectangle backgroundControl;

        private Storyboard storyboardFadeIn;

        public new static bool IsOpened => instance != null;

        public static void EnsureClosed()
        {
            if (isClosed)
            {
                return;
            }

            isClosed = true;
            instance?.CloseWindow();
        }

        public static void EnsureOpened()
        {
            if (!isClosed)
            {
                return;
            }

            isClosed = false;
            var windowRespawn = new WindowRespawn();
            instance = windowRespawn;

            Api.Client.UI.LayoutRootChildren.Add(instance);

            var loadingSplashScreenState = LoadingSplashScreenManager.Instance.CurrentState;
            var delay = loadingSplashScreenState == LoadingSplashScreenState.Shown
                        || loadingSplashScreenState == LoadingSplashScreenState.Showing
                            ? 0
                            : 2;
            ClientComponentTimersManager.AddAction(
                delaySeconds: delay,
                action: () =>
                        {
                            windowRespawn.Window.IsCached = false;

                            if (isClosed)
                            {
                                if (ReferenceEquals(instance, windowRespawn))
                                {
                                    instance = null;
                                    Api.Client.UI.LayoutRootChildren.Remove(instance);
                                }

                                return;
                            }

                            instance.Window.Open();
                        });
        }

        protected override void InitControlWithWindow()
        {
            this.DataContext = new ViewModelWindowRespawn();
            this.backgroundControl = new Rectangle()
            {
                Fill = new SolidColorBrush(Colors.Black)
            };

            // special hack for NoesisGUI animation completed event
            this.backgroundControl.Tag = this;

            Api.Client.UI.LayoutRootChildren.Add(this.backgroundControl);

            var loadingSplashScreenState = LoadingSplashScreenManager.Instance.CurrentState;
            if (loadingSplashScreenState == LoadingSplashScreenState.Shown
                || loadingSplashScreenState == LoadingSplashScreenState.Showing)
            {
                // no fade-in required
                return;
            }

            this.storyboardFadeIn = AnimationHelper.CreateStoryboard(
                this.backgroundControl,
                durationSeconds: 3,
                from: 0,
                to: 1,
                propertyName: OpacityProperty.Name);
            this.storyboardFadeIn.Begin(this.backgroundControl);
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            WindowsManager.BringToFront(this.Window);
            Panel.SetZIndex(this.backgroundControl, this.Window.CurrentZIndex - 1);
            Menu.CloseAll();
        }

        protected override void WindowClosed()
        {
            base.WindowClosed();

            this.storyboardFadeIn?.Stop(this.backgroundControl);

            AnimationHelper.CreateStoryboard(
                               this.backgroundControl,
                               durationSeconds: 0.667,
                               from: this.backgroundControl.Opacity,
                               to: 0,
                               propertyName: OpacityProperty.Name,
                               onCompleted: this.BackgroundFadeOutCompleted)
                           .Begin(this.backgroundControl);

            if (instance == this)
            {
                instance = null;
            }
        }

        private void BackgroundFadeOutCompleted()
        {
            Api.Client.UI.LayoutRootChildren.Remove(this.backgroundControl);
        }
    }
}