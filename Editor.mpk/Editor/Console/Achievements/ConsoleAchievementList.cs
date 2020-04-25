namespace AtomicTorch.CBND.CoreMod.Editor.Console.Achievements
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ConsoleAchievementList : BaseConsoleCommand
    {
        public override string Description => "List unlocked achievements of a player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ClientAndServerOperatorOnly;

        public override string Name => "achievement.listUnlocked";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var achievements = player.SharedGetAchievements();
            return "Unlocked achievements for "
                   + player
                   + ":"
                   + Environment.NewLine
                   + achievements.UnlockedAchievements.Select(a => "* " + a.Achievement.ShortId)
                                 .GetJoinedString(Environment.NewLine);
        }
    }
}