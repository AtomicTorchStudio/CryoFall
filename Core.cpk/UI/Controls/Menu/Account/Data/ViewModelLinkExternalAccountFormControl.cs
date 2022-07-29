namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Account.Data
{
    using System.Windows;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Steam.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelLinkExternalAccountFormControl : BaseViewModel
    {
        public ViewModelLinkExternalAccountFormControl()
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

        public string Title
            => Api.Client.ExternalApi.IsSteamClient
                   ? CoreStrings.LinkSteamAccountForm_Title
                   : CoreStrings.LinkEpicAccountForm_Title;

        protected override void DisposeViewModel()
        {
            if (this.PasswordInputControl is not null)
            {
                this.PasswordInputControl.PreviewKeyUp -= this.PasswordInputControlLinkingFormKeyUp;
            }

            base.DisposeViewModel();
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