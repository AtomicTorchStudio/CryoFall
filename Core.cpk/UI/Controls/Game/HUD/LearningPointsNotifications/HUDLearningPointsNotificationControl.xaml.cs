namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.LearningPointsNotifications
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.Systems.Cursor;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using Menu = AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu.Menu;

    public partial class HUDLearningPointsNotificationControl : BaseUserControl
    {
        public static readonly DependencyProperty CurrentLearningPointsProperty =
            DependencyProperty.Register(nameof(CurrentLearningPoints),
                                        typeof(uint),
                                        typeof(HUDLearningPointsNotificationControl),
                                        new PropertyMetadata(default(uint)));

        public static readonly DependencyProperty RequiredHeightProperty =
            DependencyProperty.Register(nameof(RequiredHeight),
                                        typeof(float),
                                        typeof(HUDLearningPointsNotificationControl),
                                        new PropertyMetadata(default(float)));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text),
                                        typeof(string),
                                        typeof(HUDLearningPointsNotificationControl),
                                        new PropertyMetadata(default(string)));

        private static readonly SolidColorBrush BrushBackgroundGreen
            = Api.Client.UI.GetApplicationResource<SolidColorBrush>("BrushColorGreen6");

        private static readonly SolidColorBrush BrushBackgroundRed
            = Api.Client.UI.GetApplicationResource<SolidColorBrush>("BrushColorRed6");

        private Storyboard storyboardHide;

        private Storyboard storyboardShow;

        public uint CurrentLearningPoints
        {
            get => (uint)this.GetValue(CurrentLearningPointsProperty);
            set => this.SetValue(CurrentLearningPointsProperty, value);
        }

        public bool IsHiding { get; set; }

        public float RequiredHeight
        {
            get => (float)this.GetValue(RequiredHeightProperty);
            set => this.SetValue(RequiredHeightProperty, value);
        }

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }

        public static HUDLearningPointsNotificationControl Create(long deltaCount, uint currentLearningPoints)
        {
            var deltaCountText = deltaCount > 0 ? '+' + deltaCount.ToString() : deltaCount.ToString();

            return new HUDLearningPointsNotificationControl()
            {
                Text = string.Format("{0} {1}",
                                     deltaCountText,
                                     CoreStrings.LearningPointsAbbreviation),
                CurrentLearningPoints = currentLearningPoints,
                Foreground = deltaCount > 0
                                 ? BrushBackgroundGreen
                                 : BrushBackgroundRed
            };
        }

        public void Hide(bool quick)
        {
            if (quick)
            {
                this.storyboardHide.SpeedRatio = 6.5;
            }

            if (this.IsHiding)
            {
                // already hiding
                return;
            }

            this.IsHiding = true;

            if (!this.isLoaded)
            {
                this.RemoveControl();
                return;
            }

            //Api.Logger.WriteDev($"Hiding notification: {this.viewModel.ProtoItem.Name}: {this.viewModel.DeltaCountText}");
            this.storyboardShow.Stop();
            this.storyboardHide.Begin();
        }

        protected override void InitControl()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.storyboardShow = this.GetResource<Storyboard>("StoryboardShow");
            this.storyboardHide = this.GetResource<Storyboard>("StoryboardHide");
        }

        protected override void OnLoaded()
        {
            this.RequiredHeight = (float)this.ActualHeight;

            if (IsDesignTime)
            {
                return;
            }

            this.storyboardShow.Begin();
            this.storyboardHide.Completed += this.StoryboardHideCompletedHandler;

            this.MouseEnter += this.MouseEnterHandler;
            this.MouseLeftButtonDown += this.MouseLeftButtonHandler;
            this.MouseRightButtonDown += this.MouseRightButtonHandler;
            this.MouseLeave += this.MouseLeaveHandler;
        }

        protected override void OnUnloaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.storyboardHide.Completed -= this.StoryboardHideCompletedHandler;

            this.MouseEnter -= this.MouseEnterHandler;
            this.MouseLeftButtonDown -= this.MouseLeftButtonHandler;
            this.MouseRightButtonDown -= this.MouseRightButtonHandler;
            this.MouseLeave -= this.MouseLeaveHandler;

            this.RemoveControl();
        }

        private void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            ClientCursorSystem.CurrentCursorId = CursorId.InteractionPossible;
        }

        private void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            ClientCursorSystem.CurrentCursorId = CursorId.Default;
        }

        private void MouseLeftButtonHandler(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            this.Hide(quick: true);
            Menu.Toggle<WindowTechnologies>();
        }

        private void MouseRightButtonHandler(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            this.Hide(quick: true);
        }

        private void RemoveControl()
        {
            var parent = this.Parent as Panel;
            parent?.Children.Remove(this);
        }

        private void StoryboardHideCompletedHandler(object sender, EventArgs e)
        {
            this.RemoveControl();
        }
    }
}