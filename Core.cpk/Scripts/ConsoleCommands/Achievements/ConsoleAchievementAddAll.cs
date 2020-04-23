namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Achievements
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Achievements;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleAchievementAddAll : BaseConsoleCommand
    {
        public override string Description => "Add all achievements to a player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "achievement.addAll";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var achievements = player.SharedGetAchievements();
            foreach (var achievement in AchievementsSystem.AllAchievements)
            {
                achievements.ServerTryAddAchievement(achievement, isUnlocked: true);
            }

            return null;
        }
    }
}