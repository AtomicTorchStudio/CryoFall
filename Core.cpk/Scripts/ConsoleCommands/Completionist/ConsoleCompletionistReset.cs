namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Completionist
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleCompletionistReset : BaseConsoleCommand
    {
        public override string Description => "Reset completionist entries for player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "completionist.reset";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var completionistData = PlayerCharacter.GetPrivateState(player).CompletionistData;
            completionistData.ServerReset();
            return null;
        }
    }
}