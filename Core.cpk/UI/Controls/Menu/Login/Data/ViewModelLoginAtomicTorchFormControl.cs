namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Login.Data
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelLoginAtomicTorchFormControl : BaseViewModel
    {
        public const string ErrorAwaitingLoginResponse = "Awaiting login response.";

        public const string ErrorEnterEmail = "Please enter your email address.";

        public const string ErrorEnterPassword = "Please enter your password.";

        public ViewModelLoginAtomicTorchFormControl()
        {
            if (IsDesignTime)
            {
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                this.Email = "abc@example.com";
                this.PasswordInputControl = new TextBox { Text = new string('*', 6) };
                return;
            }

            this.PasswordInputControl = Client.UI.CreateSecurePasswordInputControl();
            this.PasswordInputControl.PreviewKeyUp += this.PasswordInputControlPreviewKeyUp;
        }

        public BaseCommand CommandLogin => new ActionCommand(this.ExecuteCommandLogin);

        public BaseCommand CommandQuit => new ActionCommand(this.ExecuteCommandQuit);

        public string Email { get; set; }

        [ViewModelNotAutoDisposeField]
        public FrameworkElement PasswordInputControl { get; }

        public bool RememberMe { get; set; } = true;

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            if (this.PasswordInputControl is not null)
            {
                this.PasswordInputControl.PreviewKeyUp -= this.PasswordInputControlPreviewKeyUp;
            }
        }

        private void ExecuteCommandLogin()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.Email))
                {
                    throw new Exception(ErrorEnterEmail);
                }

                if (Client.MasterServer.IsEmptyPassword(this.PasswordInputControl))
                {
                    throw new Exception(ErrorEnterPassword);
                }

                if (Client.MasterServer.IsAwaitingLoginResponse)
                {
                    throw new Exception(ErrorAwaitingLoginResponse);
                }

                Client.MasterServer.LoginPlayer(this.Email, this.PasswordInputControl, this.RememberMe);
            }
            catch (Exception ex)
            {
                DialogWindow.ShowDialog(title: CoreStrings.TitleAttention,
                                        ex.Message,
                                        zIndexOffset: 9002);
            }
        }

        private void ExecuteCommandQuit()
        {
            Client.Core.Quit();
        }

        private void PasswordInputControlPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // submit
                e.Handled = true;
                this.ExecuteCommandLogin();
            }
        }
    }
}