namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.DemoVersion
{
    using System.Windows;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.DemoVersion.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDDemoVersionInfo : BaseUserControl
    {
        private const double ExpandOrCollapseDelay = 0.1;

        private int expandedStateRefreshScheduledNumber;

        private FrameworkElement layoutRoot;

        private ViewModelHUDDemoVersionInfo viewModel;

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<FrameworkElement>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.UpdateLayout();

            this.viewModel = new ViewModelHUDDemoVersionInfo();
            this.viewModel.RequiredHeight = (float)this.GetByName<FrameworkElement>("Description")
                                                       .ActualHeight;
            this.DataContext = this.viewModel;

            this.expandedStateRefreshScheduledNumber = 0;

            VisualStateManager.GoToElementState(this.layoutRoot,
                                                "Collapsed",
                                                useTransitions: false);

            this.MouseEnter += this.MouseEnterOrLeaveHandler;
            this.MouseLeave += this.MouseEnterOrLeaveHandler;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;

            this.MouseEnter -= this.MouseEnterOrLeaveHandler;
            this.MouseLeave -= this.MouseEnterOrLeaveHandler;
        }

        private void MouseEnterOrLeaveHandler(object sender, MouseEventArgs e)
        {
            var refreshNumber = ++this.expandedStateRefreshScheduledNumber;
            ClientTimersSystem.AddAction(ExpandOrCollapseDelay,
                                         () => this.RefreshState(refreshNumber));
        }

        private void RefreshState(int refreshNumber)
        {
            if (this.expandedStateRefreshScheduledNumber != refreshNumber)
            {
                return;
            }

            VisualStateManager.GoToElementState(this.layoutRoot,
                                                this.IsMouseOver
                                                    ? "Expanded"
                                                    : "Collapsed",
                                                useTransitions: true);
        }
    }
}