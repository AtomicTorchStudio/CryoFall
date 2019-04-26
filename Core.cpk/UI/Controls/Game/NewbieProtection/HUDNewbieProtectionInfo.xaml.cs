namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.NewbieProtection
{
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.NewbieProtection.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDNewbieProtectionInfo : BaseUserControl
    {
        private Storyboard storyboardHide;

        private Storyboard storyboardShow;

        private ViewModelHUDNewbieProtectionInfo viewModel;

        protected override void InitControl()
        {
            this.storyboardShow = this.GetResource<Storyboard>("StoryboardShow");
            this.storyboardHide = this.GetResource<Storyboard>("StoryboardHide");

            this.MouseEnter += this.MouseEnterOrLeaveHandler;
            this.MouseLeave += this.MouseEnterOrLeaveHandler;

            NewbieProtectionSystem.ClientNewbieProtectionTimeRemainingReceived += _ => this.Refresh();
        }

        protected override void OnLoaded()
        {
            this.UpdateLayout();
            this.storyboardHide.Begin();

            this.viewModel = new ViewModelHUDNewbieProtectionInfo();
            this.viewModel.RequiredHeight = (float)this.GetByName<FrameworkElement>("Description")
                                                       .ActualHeight;
            this.DataContext = this.viewModel;

            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        private void MouseEnterOrLeaveHandler(object sender, MouseEventArgs e)
        {
            if (this.IsMouseOver)
            {
                this.storyboardHide.Stop();
                this.storyboardShow.Begin();
            }
            else
            {
                this.storyboardShow.Stop();
                this.storyboardHide.Begin();
            }
        }

        private void Refresh()
        {
            this.viewModel?.Setup(NewbieProtectionSystem.ClientNewbieProtectionTimeRemaining);
        }
    }
}