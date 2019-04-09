namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Steam.Data
{
    using System;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelSteamAccountLinkingWelcome : BaseViewModel
    {
        private readonly Action callbackClose;

        private readonly Action callbackResizeWindow;

        private Modes mode;

        public ViewModelSteamAccountLinkingWelcome(
            Action callbackResizeWindow = null,
            Action callbackClose = null)
        {
            this.callbackResizeWindow = callbackResizeWindow;
            this.callbackClose = callbackClose;
        }

        public enum Modes
        {
            ModeSelection,

            AccountLinking,

            AccountRegistration
        }

        public BaseCommand CommandClose => new ActionCommand(this.callbackClose);

        public BaseCommand CommandOpenAccountLinkingForm
            => new ActionCommand(() => this.SetMode(Modes.AccountLinking));

        public BaseCommand CommandOpenRegisterAccountForm
            => new ActionCommand(() => this.SetMode(Modes.AccountRegistration));

        public BaseCommand CommandProceedWithoutAccount
            => new ActionCommand(this.ExecuteCommandProceedWithoutAccount);

        public BaseCommand CommandQuit
            => new ActionCommand(this.ExecuteCommandQuit);

        public BaseCommand CommandResetMenu
            => new ActionCommand(() => this.SetMode(Modes.ModeSelection));

        public Visibility ViewVisibilityAccountLinkingForm
            => this.ToVisibility(this.mode == Modes.AccountLinking);

        public Visibility ViewVisibilityAccountRegistrationForm =>
            this.ToVisibility(this.mode == Modes.AccountRegistration);

        public Visibility ViewVisibilityModeSelection
            => this.ToVisibility(this.mode == Modes.ModeSelection);

        private void ExecuteCommandProceedWithoutAccount()
        {
            Client.SteamApi.RegisterSteamOnlyAccount();
        }

        private void ExecuteCommandQuit()
        {
            Client.Core.Quit();
        }

        private void SetMode(Modes newMode)
        {
            if (this.mode == newMode)
            {
                return;
            }

            this.mode = newMode;
            this.NotifyPropertyChanged(nameof(this.ViewVisibilityModeSelection));
            this.NotifyPropertyChanged(nameof(this.ViewVisibilityAccountLinkingForm));
            this.NotifyPropertyChanged(nameof(this.ViewVisibilityAccountRegistrationForm));

            this.callbackResizeWindow?.Invoke();
        }

        private Visibility ToVisibility(bool b)
        {
            return b ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}