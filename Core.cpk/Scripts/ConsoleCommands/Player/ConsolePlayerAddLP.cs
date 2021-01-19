// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerAddLP : BaseConsoleCommand
    {
        public override string Description => "Add technology learning points value for a player character.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.addLP";

        public string Execute(uint learningPoints, [CurrentCharacterIfNull] ICharacter player = null)
        {
            var technologies = player.SharedGetTechnologies();

            technologies.ServerAddLearningPoints(learningPoints, allowModifyingByStatsAndRates: false);
            return $"{player} added {learningPoints} LP and now has {technologies.LearningPoints} LP.";
        }
    }
}