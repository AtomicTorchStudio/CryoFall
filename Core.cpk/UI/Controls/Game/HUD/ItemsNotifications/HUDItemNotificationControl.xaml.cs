namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.ItemsNotifications
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.Systems.Cursor;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.ItemsNotifications.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using Menu = AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu.Menu;

    public partial class HUDItemNotificationControl : BaseUserControl
    {
        private Storyboard storyboardHide;

        private Storyboard storyboardShow;

        private ViewModelHUDItemNotificationControl viewModel;

        public int DeltaCount => this.viewModel?.DeltaCount ?? 0;

        public bool IsHiding { get; set; }

        public IProtoItem ProtoItem => this.viewModel?.ProtoItem;

        public ViewModelHUDItemNotificationControl ViewModel => this.viewModel;

        public static HUDItemNotificationControl Create(IProtoItem protoItem, int deltaCount)
        {
            return new HUDItemNotificationControl()
            {
                viewModel = new ViewModelHUDItemNotificationControl(protoItem, deltaCount)
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
                this.viewModel = new ViewModelHUDItemNotificationControl();
                return;
            }

            this.storyboardShow = this.GetResource<Storyboard>("StoryboardShow");
            this.storyboardHide = this.GetResource<Storyboard>("StoryboardHide");
        }

        protected override void OnLoaded()
        {
            this.viewModel.RequiredHeight = (float)this.ActualHeight;
            this.DataContext = this.viewModel;

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
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;

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
            Menu.Toggle<WindowInventory>();
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