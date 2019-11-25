namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.DemoVersion
{
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.DemoVersion.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDDemoVersionInfo : BaseUserControl
    {
        private Storyboard storyboardHide;

        private Storyboard storyboardShow;

        private ViewModelHUDDemoVersionInfo viewModel;

        protected override void InitControl()
        {
            this.storyboardShow = this.GetResource<Storyboard>("StoryboardShow");
            this.storyboardHide = this.GetResource<Storyboard>("StoryboardHide");

            this.MouseEnter += this.MouseEnterOrLeaveHandler;
            this.MouseLeave += this.MouseEnterOrLeaveHandler;
        }

        protected override void OnLoaded()
        {
            this.UpdateLayout();
            this.storyboardHide.Begin();

            this.viewModel = new ViewModelHUDDemoVersionInfo();
            this.viewModel.RequiredHeight = (float)this.GetByName<FrameworkElement>("Description")
                                                       .ActualHeight;
            this.DataContext = this.viewModel;
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
    }
}