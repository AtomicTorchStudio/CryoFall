namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelNewbieProtectionControl : BaseViewModel
    {
        public ViewModelNewbieProtectionControl()
        {
            NewbieProtectionSystem.ClientNewbieProtectionTimeRemainingReceived +=
                this.NewbieProtectionTimeRemainingReceivedHandler;
            this.Refresh();
        }

        public BaseCommand CommandCancelNewbieProtection
            => new ActionCommand(this.ExecuteCommandCancelNewbieProtection);

        public Visibility Visibility { get; private set; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            NewbieProtectionSystem.ClientNewbieProtectionTimeRemainingReceived -=
                this.NewbieProtectionTimeRemainingReceivedHandler;
        }

        private void ExecuteCommandCancelNewbieProtection()
        {
            DialogWindow.ShowDialog(
                CoreStrings.QuestionAreYouSure,
                NewbieProtectionSystem.Dialog_CancelNewbieProtection,
                okAction: NewbieProtectionSystem.ClientDisableNewbieProtection,
                cancelAction: () => { },
                focusOnCancelButton: true);
        }

        private void NewbieProtectionTimeRemainingReceivedHandler(double timeRemains)
        {
            this.Refresh();
        }

        private void Refresh()
        {
            this.Visibility = NewbieProtectionSystem.ClientNewbieProtectionTimeRemaining > 0
                                  ? Visibility.Visible
                                  : Visibility.Collapsed;
        }
    }
}