namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Steam.Data
{
    using System;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public static class SteamAccountRegistrationHelper
    {
        public const string DialogCannotLinkAccount_IncorrectEmailOrPassword =
            "The email address or password you've entered is incorrect. If you're sure that you've entered the correct email, please try again or use the Reset password feature on AtomicTorch.com.";

        public const string DialogCannotLinkAccount_MessageEmailAlreadyUsed =
            "Entered email is already used by another account. Please use a different email address.";

        public const string DialogCannotLinkAccount_MessageEmailNotActivated =
            @"The email address you've entered is not activated.
              [br]Please open your email account and click on the activation link we just sent to you. (Please note that the email might be in your spam inbox).";

        public const string DialogCannotLinkAccount_Title =
            "Cannot link account";

        public const string DialogCannotRegister_MessageAlreadyLinkedToAnotherAtomicTorchAccount =
            "Steam account is already linked to another account.";

        public const string DialogCannotRegister_MessageAlreadyLinkedToAnotherSteamId =
            "The account is already linked with another Steam ID. Please use a different AtomicTorch.com account.";

        public const string DialogCannotRegister_MessageRestartSteamClient =
            "Please try to restart the Steam client.";

        public const string DialogCannotRegister_Title =
            "Cannot register";

        public const string ErrorConfirmEmailDoesntMatch =
            "Confirm Email doesn't match the entered Email.";

        public const string ErrorEnterEmail =
            "Please enter your e-mail.";

        public const string ErrorEnterPassword =
            "Please enter your password.";

        public const string ErrorMustAcceptTermsOfService =
            "You must accept terms of service in order to continue.";

        public static async void RegisterOrLinkSteamAccount(
            string email,
            string emailConfirm,
            FrameworkElement passwordInputControlRegistrationForm,
            bool isLinkingToExistingAccount,
            bool isAcceptTermsOfService)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    throw new Exception(ErrorEnterEmail);
                }

                if (email != emailConfirm)
                {
                    throw new Exception(ErrorConfirmEmailDoesntMatch);
                }

                if (Api.Client.MasterServer.IsEmptyPassword(passwordInputControlRegistrationForm))
                {
                    throw new Exception(ErrorEnterPassword);
                }

                if (!isLinkingToExistingAccount
                    && !isAcceptTermsOfService)
                {
                    throw new Exception(ErrorMustAcceptTermsOfService);
                }

                var result = await Api.Client.ExternalApi.RegisterOrLinkAtomicTorchAccountAsync(
                                 email,
                                 passwordInputControlRegistrationForm,
                                 isLinkingToExistingAccount: isLinkingToExistingAccount,
                                 isAcceptTermsOfService: isAcceptTermsOfService);

                switch (result.Result)
                {
                    case ScriptingRegisterAtomicTorchAccountResult.ResultCode.Success:
                        Api.Client.ExternalApi.TryLoginWithExternalAccount();
                        break;

                    case ScriptingRegisterAtomicTorchAccountResult.ResultCode.SuccessNeedActivation:
                        // a error message will be displayed automatically
                        Api.Client.ExternalApi.TryLoginWithExternalAccount();
                        break;

                    case ScriptingRegisterAtomicTorchAccountResult.ResultCode.ErrorAccountLinkedtoAnotherSteamId:
                        DialogWindow.ShowMessage(
                            DialogCannotRegister_Title,
                            DialogCannotRegister_MessageAlreadyLinkedToAnotherSteamId,
                            closeByEscapeKey: true);
                        break;

                    case ScriptingRegisterAtomicTorchAccountResult.ResultCode.ErrorSteamTokenIssue:
                        DialogWindow.ShowMessage(
                            DialogCannotRegister_Title,
                            DialogCannotRegister_MessageRestartSteamClient,
                            closeByEscapeKey: true);
                        break;

                    case ScriptingRegisterAtomicTorchAccountResult.ResultCode.ErrorSteamIdAlreadyLinkedtoAnotherAccount:
                        DialogWindow.ShowMessage(
                            DialogCannotRegister_Title,
                            DialogCannotRegister_MessageAlreadyLinkedToAnotherAtomicTorchAccount,
                            closeByEscapeKey: true);
                        Api.Client.ExternalApi.TryLoginWithExternalAccount();
                        break;

                    case ScriptingRegisterAtomicTorchAccountResult.ResultCode.ErrorUnknown:
                        DialogWindow.ShowMessage(
                            DialogCannotRegister_Title,
                            result.ErrorMessage,
                            closeByEscapeKey: true);
                        break;

                    case ScriptingRegisterAtomicTorchAccountResult.ResultCode.ErrorEmailUsedByAnotherAccount:
                        DialogWindow.ShowMessage(
                            DialogCannotLinkAccount_Title,
                            DialogCannotLinkAccount_MessageEmailAlreadyUsed,
                            closeByEscapeKey: true);
                        break;

                    case ScriptingRegisterAtomicTorchAccountResult.ResultCode.ErrorCannotLinkToNonActivatedAccount:
                        DialogWindow.ShowMessage(
                            DialogCannotLinkAccount_Title,
                            DialogCannotLinkAccount_MessageEmailNotActivated,
                            closeByEscapeKey: true);
                        break;

                    case ScriptingRegisterAtomicTorchAccountResult.ResultCode
                                                                  .ErrorCannotLinkToExistingAccountEmailOrPasswordIncorrect
                        :
                        DialogWindow.ShowMessage(
                            DialogCannotLinkAccount_Title,
                            DialogCannotLinkAccount_IncorrectEmailOrPassword,
                            closeByEscapeKey: true);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception ex)
            {
                DialogWindow.ShowDialog(title: CoreStrings.TitleAttention,
                                        text: ex.Message,
                                        zIndexOffset: 9002);
            }
        }
    }
}