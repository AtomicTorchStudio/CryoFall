namespace AtomicTorch.CBND.CoreMod.Helpers.Client.Server
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Login;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public static class MasterLoginFailedDialogHelper
    {
        public const string AccountBlocked = "Your account is blocked.";

        public const string DefaultTitle = "Login failed";

        public const string EmailWithTheActivation_Message =
            @"An email with the activation link has been sent to your
              [br]account and should arrive shortly.
              [br]Please open your email account and click on the activation link, then return to the game to proceed.";

        public const string EmailWithTheActivation_Title = "Your new account requires activation!";

        public const string GameNotOwned = "You don't own the game on your account.";

        public const string IncorrectEmailOrPassword = "Incorrect email or password.";

        public const string UnknownError = "Unknown error. Please try again.";

        public const string UserHasAssignedDifferentSteamId_Message =
            @"The provided user details already have a (different) Steam account associated with them.
              [br] You cannot associate this AtomicTorch.com account with the current Steam account.";

        public const string UserHasAssignedDifferentSteamId_Title = "Cannot link accounts";

        public const string UsernameInvalid_Title = "Invalid username";

        public const string UsernameRequirements =
            @"Username requirements:
              [*] Must be [b]3[/b] to [b]24[/b] characters in length
              [*] [b]Can[/b] contain letters, numbers, underscores and hyphens
              [*] [b]Cannot[/b] contain spaces or any other characters
              [*] Must start with a [b]letter[/b]
              [*] Must end with a [b]letter[/b] or [b]number[/b]
              [*] Must [b]not[/b] contain several [b]-[/b] or [b]_[/b] characters in a row";

        public const string UsernameUsed_Message
            = @"The selected username is already used by someone.
  [br]Please pick another username.";

        public const string UsernameUsed_Title = "Username is already used";

        public static void ShowDialog(MasterClientLoginErrorCode errorCode)
        {
            string errorMessage;
            var title = DefaultTitle;

            switch (errorCode)
            {
                case MasterClientLoginErrorCode.TokenDisabled:
                    // the master token has been expired and user need to login again
                    // don't display any errors
                    return;

                case MasterClientLoginErrorCode.LoginOrPasswordIncorrect:
                    errorMessage = IncorrectEmailOrPassword;
                    break;

                case MasterClientLoginErrorCode.GameNotOwned:
                    errorMessage = GameNotOwned;
                    break;

                case MasterClientLoginErrorCode.AccountBlocked:
                    errorMessage = AccountBlocked;
                    break;

                case MasterClientLoginErrorCode.AccountNotActivated:
                    DialogWindow.ShowDialog(
                        EmailWithTheActivation_Title,
                        EmailWithTheActivation_Message,
                        okText: Api.Client.ExternalApi.IsExternalClient
                                    ? CoreStrings.Button_Continue
                                    : CoreStrings.Button_OK,
                        okAction: () =>
                                  {
                                      if (Api.Client.ExternalApi.IsExternalClient)
                                      {
                                          Api.Client.ExternalApi.TryLoginWithExternalAccount();
                                      }
                                  },
                        textAlignment: TextAlignment.Left);
                    return;

                case MasterClientLoginErrorCode.UsernameNotSelected:
                    MenuLogin.SetDisplayed(MenuLoginMode.SelectUsername);
                    return;

                case MasterClientLoginErrorCode.UsernameInvalid:
                    MenuLogin.SetDisplayed(MenuLoginMode.SelectUsername);
                    title = UsernameInvalid_Title;
                    errorMessage = UsernameRequirements;
                    break;

                case MasterClientLoginErrorCode.UsernameUsed:
                    MenuLogin.SetDisplayed(MenuLoginMode.SelectUsername);
                    title = UsernameUsed_Title;
                    errorMessage = UsernameUsed_Message;
                    break;

                case MasterClientLoginErrorCode.SteamUserNotAssigned:
                    // display regular login menu (the game will detect this is a Steam version and properly handle that)
                    MenuLogin.SetDisplayed(MenuLoginMode.Login);
                    return;

                case MasterClientLoginErrorCode.UserHasAssignedDifferentSteamId:
                    title = UserHasAssignedDifferentSteamId_Title;
                    errorMessage = UserHasAssignedDifferentSteamId_Message;
                    break;

                case MasterClientLoginErrorCode.UnknownError:
                default:
                    MenuLogin.SetDisplayed(
                        Api.Client.ExternalApi.IsSteamClient
                            ? MenuLoginMode.SteamError
                            : MenuLoginMode.EpicLauncherError);
                    errorMessage = UnknownError;
                    break;
            }

            DialogWindow.ShowDialog(title,
                                    errorMessage,
                                    zIndexOffset: 9002,
                                    textAlignment: TextAlignment.Left);
        }
    }
}