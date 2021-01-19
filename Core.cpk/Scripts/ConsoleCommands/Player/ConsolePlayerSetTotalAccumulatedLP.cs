// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerSetTotalAccumulatedLP : BaseConsoleCommand
    {
        public override string Description =>
            "Set total accumulated technology learning points value for a player character.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.setTotalAccumulatedLP";

        public string Execute(uint learningPoints, [CurrentCharacterIfNull] ICharacter player = null)
        {
            var technologies = player.SharedGetTechnologies();
            technologies.ServerSetTotalAccumulatedLearningPoints(learningPoints);
            return string.Format("{0} total accumulated learning points reset to {1}.",
                                 player,
                                 technologies.LearningPointsAccumulatedTotal);
        }
    }
}