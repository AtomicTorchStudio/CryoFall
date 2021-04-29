namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.NewbieProtection
{
    using System.Windows;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.NewbieProtection.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Social;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDNewbieProtectionInfo : BaseUserControl
    {
        private const double ExpandOrCollapseDelay = 0.1;

        private int expandedStateRefreshScheduledNumber;

        private FrameworkElement layoutRoot;

        private ViewModelHUDNewbieProtectionInfo viewModel;

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<FrameworkElement>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.UpdateLayout();

            this.viewModel = new ViewModelHUDNewbieProtectionInfo();
            this.viewModel.RequiredHeight = (float)this.GetByName<FrameworkElement>("Description")
                                                       .ActualHeight;
            this.DataContext = this.viewModel;

            this.expandedStateRefreshScheduledNumber = 0;
            this.RefreshTimeRemaining();

            VisualStateManager.GoToElementState(this.layoutRoot,
                                                "Collapsed",
                                                useTransitions: false);

            NewbieProtectionSystem.ClientNewbieProtectionTimeRemainingReceived
                += this.NewbieProtectionTimeRemainingReceivedHandler;
            this.MouseEnter += this.MouseEnterOrLeaveHandler;
            this.MouseLeave += this.MouseEnterOrLeaveHandler;
            this.MouseDown += MouseDownHandler;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;

            NewbieProtectionSystem.ClientNewbieProtectionTimeRemainingReceived
                -= this.NewbieProtectionTimeRemainingReceivedHandler;
            this.MouseEnter -= this.MouseEnterOrLeaveHandler;
            this.MouseLeave -= this.MouseEnterOrLeaveHandler;
            this.MouseDown -= MouseDownHandler;
        }

        private static void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            Menu.Open<WindowSocial>();
        }

        private void MouseEnterOrLeaveHandler(object sender, MouseEventArgs e)
        {
            var refreshNumber = ++this.expandedStateRefreshScheduledNumber;
            ClientTimersSystem.AddAction(ExpandOrCollapseDelay,
                                         () => this.RefreshState(refreshNumber));
        }

        private void NewbieProtectionTimeRemainingReceivedHandler(double obj)
        {
            this.RefreshTimeRemaining();
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

        private void RefreshTimeRemaining()
        {
            this.viewModel?.Setup(NewbieProtectionSystem.ClientNewbieProtectionTimeRemaining);
        }
    }
}