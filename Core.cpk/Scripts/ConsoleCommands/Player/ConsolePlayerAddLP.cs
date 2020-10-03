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

        public string Execute(ushort learningPoints, [CurrentCharacterIfNull] ICharacter player = null)
        {
            var technologies = player.SharedGetTechnologies();

            // we're not using this method as it will add more LP than asked
            // due to Learning skill and Savory food buffs 
            //technologies.ServerAddLearningPoints(learningPoints);

            technologies.ServerSetLearningPoints(technologies.LearningPoints + learningPoints);
            return $"{player} added {learningPoints} LP and now has {technologies.LearningPoints} LP.";
        }
    }
}