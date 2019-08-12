namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ScreenshotNotification : BaseUserControl
    {
        public static readonly DependencyProperty FileNameProperty =
            DependencyProperty.Register(nameof(FileName),
                                        typeof(string),
                                        typeof(ScreenshotNotification),
                                        new PropertyMetadata(default(string), FileNamePropertyChangedHandler));

        public static readonly DependencyProperty ScreenshotReadyMessageProperty =
            DependencyProperty.Register(nameof(ScreenshotReadyMessage),
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

        public string ScreenshotReadyMessage
        {
            get => (string)this.GetValue(ScreenshotReadyMessageProperty);
            set => this.SetValue(ScreenshotReadyMessageProperty, value);
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
            ClientUpdateHelper.UpdateCallback += this.Update;
            this.storyboardShow.Begin();
            ClientTimersSystem.AddAction(delaySeconds: 5,
                                                   () => this.StartHideAnimation(quick: false));
        }

        protected override void OnUnloaded()
        {
            this.MouseLeftButtonUp -= this.ClickHandler;
            ClientUpdateHelper.UpdateCallback -= this.Update;
        }

        private static void FileNamePropertyChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ScreenshotNotification)d).ScreenshotReadyMessage
                = string.Format(CoreStrings.ScreenshotNotification_NotificationFormat, e.NewValue);
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
            ClientTimersSystem.AddAction(delaySeconds: 3, this.Remove);
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