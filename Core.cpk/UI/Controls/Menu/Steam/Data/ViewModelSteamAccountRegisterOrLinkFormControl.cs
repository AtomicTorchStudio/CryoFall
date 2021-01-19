namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Steam.Data
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelSteamAccountRegisterOrLinkFormControl : BaseViewModel
    {
        public const string EmailPleaseUseAnotherEmailService =
            "It looks like you are trying to register using an email account from a server owned by Microsoft. Unfortunately, Microsoft outright [b]blocks[/b] any emails that aren't from large providers, effectively preventing us from sending any emails to your account. Please use any other email service, such as [b]gmail.com[/b].";

        // we have trouble delivering emails to these Microsoft domains
        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        private static readonly string[] NotRecommendEmailRegistrationDomains =
        {
            "live.",
            "hotmail.",
            "msn.",
            "outlook."
        };

        private string email;

        public ViewModelSteamAccountRegisterOrLinkFormControl()
        {
            this.PasswordInputControl = Client.UI.CreateSecurePasswordInputControl();
        }

        public bool CanRegister { get; private set; }

        public BaseCommand CommandRegisterAccount
            => new ActionCommand(this.ExecuteCommandRegisterAccount);

        public string Email
        {
            get => this.email;
            set
            {
                if (this.email == value)
                {
                    return;
                }

                this.email = value;
                this.NotifyThisPropertyChanged();

                if (NotRecommendEmailRegistrationDomains.Any(
                    domain => this.email.IndexOf(domain, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    this.EmailRegistrationErrorMessage = EmailPleaseUseAnotherEmailService;
                    this.CanRegister = false;
                }
                else
                {
                    this.EmailRegistrationErrorMessage = null;
                    this.CanRegister = !string.IsNullOrEmpty(this.email);
                }
            }
        }

        public string EmailConfirm { get; set; }

        public string EmailRegistrationErrorMessage { get; private set; }

        public bool IsAcceptTermsOfService { get; set; }

        [ViewModelNotAutoDisposeField]
        public FrameworkElement PasswordInputControl { get; }

        private void ExecuteCommandRegisterAccount()
        {
            SteamAccountRegistrationHelper.RegisterOrLinkSteamAccount(
                this.Email,
                this.EmailConfirm,
                this.PasswordInputControl,
                false,
                this.IsAcceptTermsOfService);
        }
    }
}