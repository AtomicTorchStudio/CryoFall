namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Core;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Timer;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ScreenshotNotification : BaseUserControl
    {
        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register("FileName",
                                        typeof(string),
                                        typeof(ScreenshotNotification),
                                        new PropertyMetadata(default(string)));

        private static readonly IRenderingClientService RenderingClientService = Api.Client.Rendering;

        private static bool isInitialized;

        private bool isHiding;

        private Storyboard storyboardFadeOut;

        private Storyboard storyboardShow;

        public string FileName
        {
            get => (string)this.GetValue(FileNameProperty);
            set => this.SetValue(FileNameProperty, value);
        }

        public static void InitializeScreenshotOverlaySystem()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;

            RenderingClientService.ScreenshotReady += ScreenshotReadyHandler;
        }

        protected override void InitControl()
        {
            base.InitControl();

            this.storyboardShow = this.GetResource<Storyboard>("StoryboardShow");
            this.storyboardFadeOut = this.GetResource<Storyboard>("StoryboardFadeOut");
        }

        protected override void OnLoaded()
        {
            this.MouseLeftButtonUp += this.ClickHandler;
            ClientComponentUpdateHelper.UpdateCallback += this.Update;
            this.storyboardShow.Begin();
            ClientComponentTimersManager.AddAction(delaySeconds: 5,
                                                   () => this.StartHideAnimation(quick: false));
        }

        protected override void OnUnloaded()
        {
            this.MouseLeftButtonUp -= this.ClickHandler;
            ClientComponentUpdateHelper.UpdateCallback -= this.Update;
        }

        private static void ScreenshotReadyHandler(string fileName)
        {
            Api.Client.UI.LayoutRootChildren.Add(
                new ScreenshotNotification() { FileName = fileName });
        }

        private void ClickHandler(object sender, MouseButtonEventArgs e)
        {
            this.StartHideAnimation(quick: true);
            Api.Client.Core.NavigateToScreenshot(this.FileName);
        }

        private void Remove()
        {
            ((Panel)this.Parent)?.Children.Remove(this);
        }

        private void StartHideAnimation(bool quick)
        {
            if (!this.isLoaded)
            {
                return;
            }

            if (quick)
            {
                this.storyboardFadeOut.SpeedRatio = 6.5;
            }

            if (this.isHiding)
            {
                return;
            }

            this.isHiding = true;
            this.storyboardFadeOut.Begin();
            ClientComponentTimersManager.AddAction(delaySeconds: 3, this.Remove);
        }

        private void Update()
        {
            if (RenderingClientService.IsCapturingScreenshot)
            {
                // capturing another screenshot - hide this notification!
                this.Remove();
            }
        }
    }
}