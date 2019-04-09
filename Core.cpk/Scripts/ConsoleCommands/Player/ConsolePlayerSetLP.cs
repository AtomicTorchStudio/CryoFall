// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerSetLP : BaseConsoleCommand
    {
        public override string Description => "Set technology learning points value for a player character.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.setLP";

        public string Execute(ushort learningPoints, [CurrentCharacterIfNull] ICharacter player = null)
        {
            var technologies = player.SharedGetTechnologies();
            technologies.ServerSetLearningPoints(learningPoints);
            return $"{player} learning points now available: {technologies.LearningPoints}.";
        }
    }
}