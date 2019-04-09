namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Services;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class GameWindowBackgroundOverlay : BaseControl
    {
        private const string StateNameHidden = "Hidden";

        private const string StateNameShown = "Shown";

        private bool isBackgroundEnabled;

        private FrameworkElement layoutRoot;

        static GameWindowBackgroundOverlay()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GameWindowBackgroundOverlay),
                new FrameworkPropertyMetadata(typeof(GameWindowBackgroundOverlay)));
        }

        public bool IsBackgroundEnabled
        {
            get => this.isBackgroundEnabled;
            set
            {
                if (this.isBackgroundEnabled == value)
                {
                    return;
                }

                this.isBackgroundEnabled = value;
                this.IsHitTestVisible = this.isBackgroundEnabled;
                this.RefreshState();
            }
        }

        protected override void InitControl()
        {
            var templateRoot = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            this.layoutRoot = templateRoot.GetByName<FrameworkElement>("LayoutRoot");
            VisualStateManager.GoToElementState(this.layoutRoot, StateNameHidden, useTransitions: false);
        }

        protected override void OnLoaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.MouseLeftButtonDown += this.MouseLeftOrRightButtonDownHandler;
            this.MouseRightButtonDown += this.MouseLeftOrRightButtonDownHandler;

            this.RefreshState();
        }

        protected override void OnUnloaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.MouseLeftButtonDown -= this.MouseLeftOrRightButtonDownHandler;
            this.MouseRightButtonDown -= this.MouseLeftOrRightButtonDownHandler;
        }

        private void GoToState(string stateName)
        {
            if (this.isLoaded)
            {
                VisualStateManager.GoToElementState(this.layoutRoot, stateName, useTransitions: true);
            }
        }

        private void MouseLeftOrRightButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            var lastOpenedWindow = WindowsManager.LastOpenedWindow;
            if (lastOpenedWindow.CloseByEscapeKey
                && lastOpenedWindow.State == GameWindowState.Opened)
            {
                lastOpenedWindow.Close(DialogResult.Cancel);
            }
        }

        private void RefreshState()
        {
            this.GoToState(this.isBackgroundEnabled ? StateNameShown : StateNameHidden);
        }
    }
}