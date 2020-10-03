namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Steam.Data
{
    using System.Windows;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelLinkSteamAccountFormControl : BaseViewModel
    {
        public ViewModelLinkSteamAccountFormControl()
        {
            this.PasswordInputControl = Client.UI.CreateSecurePasswordInputControl();
            if (this.PasswordInputControl is not null)
            {
                this.PasswordInputControl.PreviewKeyUp += this.PasswordInputControlLinkingFormKeyUp;
            }
        }

        public BaseCommand CommandLinkAccounts
            => new ActionCommand(this.ExecuteCommandLinkAccounts);

        public string Email { get; set; }

        [ViewModelNotAutoDisposeField]
        public FrameworkElement PasswordInputControl { get; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            if (this.PasswordInputControl is not null)
            {
                this.PasswordInputControl.PreviewKeyUp -= this.PasswordInputControlLinkingFormKeyUp;
            }
        }

        private void ExecuteCommandLinkAccounts()
        {
            SteamAccountRegistrationHelper.RegisterOrLinkSteamAccount(
                this.Email,
                emailConfirm: this.Email,
                this.PasswordInputControl,
                isLinkingToExistingAccount: true,
                isAcceptTermsOfService: true);
        }

        private void PasswordInputControlLinkingFormKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // submit
                e.Handled = true;
                this.ExecuteCommandLinkAccounts();
            }
        }
    }
}