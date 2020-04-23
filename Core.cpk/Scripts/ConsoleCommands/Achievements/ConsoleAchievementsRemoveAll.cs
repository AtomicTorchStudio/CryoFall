namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Achievements
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleAchievementsRemoveAll : BaseConsoleCommand
    {
        public override string Description => "Remove all achievements from a player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "achievement.removeAll";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var achievements = player.SharedGetAchievements();
            achievements.ServerReset();
            return null;
        }
    }
}