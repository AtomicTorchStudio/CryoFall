namespace AtomicTorch.CBND.CoreMod.Editor.Console.Achievements
{
    using AtomicTorch.CBND.CoreMod.Achievements;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleAchievementRemove : BaseConsoleCommand
    {
        public override string Description => "Remove achievement from a player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "achievement.remove";

        public string Execute(IProtoAchievement achievement, [CurrentCharacterIfNull] ICharacter player = null)
        {
            var achievements = player.SharedGetAchievements();
            achievements.ServerTryRemoveAchievement(achievement);
            return null;
        }
    }
}