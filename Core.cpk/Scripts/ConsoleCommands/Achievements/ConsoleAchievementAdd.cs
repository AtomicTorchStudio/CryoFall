namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Achievements
{
    using AtomicTorch.CBND.CoreMod.Achievements;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleAchievementAdd : BaseConsoleCommand
    {
        public override string Description => "Add (unlock) achievement to a player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "achievement.add";

        public string Execute(IProtoAchievement achievement, [CurrentCharacterIfNull] ICharacter player = null)
        {
            var achievements = player.SharedGetAchievements();
            achievements.ServerTryAddAchievement(achievement, isUnlocked: true);
            return null;
        }
    }
}