namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Extras.Credits
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuCredits : BaseUserControl
    {
        public static readonly DependencyProperty AutoScrollSpeedProperty =
            DependencyProperty.Register(nameof(AutoScrollSpeed),
                                        typeof(double),
                                        typeof(MenuCredits),
                                        new PropertyMetadata(1.0, DependencyPropertyChangedHandler));

        public static readonly DependencyProperty IsInputAllowedProperty =
            DependencyProperty.Register(nameof(IsInputAllowed),
                                        typeof(bool),
                                        typeof(MenuCredits),
                                        new PropertyMetadata(true, DependencyPropertyChangedHandler));

        public static readonly DependencyProperty IsLoopedScrollProperty =
            DependencyProperty.Register(nameof(IsLoopedScroll),
                                        typeof(bool),
                                        typeof(MenuCredits),
                                        new PropertyMetadata(true));

        private AutoScrollViewer autoScrollViewer;

        private ScrollViewer scrollViewer;

        public event Action ScrollFinished;

        public double AutoScrollSpeed
        {
            get => (double)this.GetValue(AutoScrollSpeedProperty);
            set => this.SetValue(AutoScrollSpeedProperty, value);
        }

        public bool IsInputAllowed
        {
            get => (bool)this.GetValue(IsInputAllowedProperty);
            set => this.SetValue(IsInputAllowedProperty, value);
        }

        public bool IsLoopedScroll
        {
            get => (bool)this.GetValue(IsLoopedScrollProperty);
            set => this.SetValue(IsLoopedScrollProperty, value);
        }

        protected override void InitControl()
        {
            this.scrollViewer = this.GetByName<ScrollViewer>("ScrollViewer");
            this.autoScrollViewer = new AutoScrollViewer(this.scrollViewer, isLoopedScroll: this.IsLoopedScroll);
        }

        protected override void OnLoaded()
        {
            this.autoScrollViewer.Finished += this.AutoScrollViewerOnFinishedHandler;
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            this.autoScrollViewer.Finished -= this.AutoScrollViewerOnFinishedHandler;
        }

        private static void DependencyPropertyChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MenuCredits)d).Refresh();
        }

        private void AutoScrollViewerOnFinishedHandler()
        {
            this.ScrollFinished?.Invoke();
        }

        private void Refresh()
        {
            if (this.autoScrollViewer is null)
            {
                return;
            }

            this.autoScrollViewer.SpeedMultiplier = this.AutoScrollSpeed;
            this.autoScrollViewer.IsInputAllowed = this.IsInputAllowed;
        }
    }
}