namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Login;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MasterServerConnectionIndicator : BaseUserControl
    {
        public const string TitleConnected = "Connected";

        public const string TitleNotConnected = "Not connected";

        private const double StoryboardHideDuration = 1;

        private const double StoryboardShowDuration = 1;

        private static readonly ViewModelMasterServerConnectionIndicator ViewModel
            = new();

        private static MasterServerConnectionIndicator instance;

        private static ConnectionState masterConnectionState;

        private FrameworkElement root;

        private Storyboard sbHide;

        private Storyboard sbShow;

        public static ConnectionState MasterConnectionState
        {
            get => masterConnectionState;
            set
            {
                if (masterConnectionState == value)
                {
                    return;
                }

                masterConnectionState = value;

                if (masterConnectionState == ConnectionState.Connecting)
                {
                    // need to display the indicator
                    if (instance is null)
                    {
                        instance = new MasterServerConnectionIndicator();
                        Api.Client.UI.LayoutRootChildren.Add(instance);
                    }

                    instance.Show(CoreStrings.TitleConnecting);
                    return;
                }

                // need to hide the indicator
                instance?.Hide(masterConnectionState == ConnectionState.Connected
                                   ? TitleConnected
                                   : TitleNotConnected);
            }
        }

        protected override void InitControl()
        {
            this.root = this.GetByName<FrameworkElement>("Root");
            this.root.DataContext = ViewModel;

            this.sbShow = AnimationHelper.CreateStoryboard(
                this.root,
                durationSeconds: StoryboardShowDuration,
                from: 0,
                to: 1,
                propertyName: OpacityProperty.Name);

            this.sbHide = AnimationHelper.CreateStoryboard(
                this.root,
                durationSeconds: StoryboardHideDuration,
                from: 1,
                to: 0,
                propertyName: OpacityProperty.Name);
        }

        protected override void OnLoaded()
        {
            MainMenuOverlay.IsHiddenChanged += this.RefreshIsHidden;
            MenuLogin.IsDisplayedChanged += this.RefreshIsHidden;
            this.sbHide.Completed += this.SbHideCompleted;

            this.RefreshIsHidden();
        }

        protected override void OnUnloaded()
        {
            MainMenuOverlay.IsHiddenChanged -= this.RefreshIsHidden;
            MenuLogin.IsDisplayedChanged -= this.RefreshIsHidden;
            this.sbHide.Completed -= this.SbHideCompleted;
        }

        private void Hide(string text)
        {
            ViewModel.Text = text;
            this.sbShow.Stop(this.root);
            this.sbHide.Begin(this.root);
        }

        private void RefreshIsHidden()
        {
            this.Visibility = MainMenuOverlay.IsHidden && !MenuLogin.IsDisplayed
                                  ? Visibility.Collapsed
                                  : Visibility.Visible;
        }

        private void SbHideCompleted(object sender, EventArgs e)
        {
            ((Panel)this.Parent).Children.Remove(this);
            if (ReferenceEquals(this, instance))
            {
                instance = null;
            }
        }

        private void Show(string text)
        {
            ViewModel.Text = text;
            this.sbHide.Stop(this.root);
            this.sbShow.Begin(this.root);
        }
    }
}