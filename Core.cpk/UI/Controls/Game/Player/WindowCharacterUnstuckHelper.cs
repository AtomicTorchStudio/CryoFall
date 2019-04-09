namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using AtomicTorch.CBND.CoreMod.Systems.CharacterUnstuck;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;

    public class WindowCharacterUnstuckHelper
    {
        public const string DialogUnstuck_Button = "Unstuck me!";

        public const string DialogUnstuck_Message =
            @"Have you walled yourself inside your house and can't get out?
              [br]Or has someone griefed you, and now you're stuck?
              [br]You can use this function to teleport your character out.
              [br]Please wait for time delay (5 minutes).";

        public const string DialogUnstuck_Title = "Unstuck";

        public static void ShowWindow()
        {
            DialogWindow.ShowDialog(
                DialogUnstuck_Title,
                ClientTextTagFormatter.NewFormattedTextBlock(
                    DialogUnstuck_Message),
                okText: DialogUnstuck_Button,
                okAction: CharacterUnstuckSystem.ClientCreateUnstuckRequest,
                hideCancelButton: false);
        }
    }
}