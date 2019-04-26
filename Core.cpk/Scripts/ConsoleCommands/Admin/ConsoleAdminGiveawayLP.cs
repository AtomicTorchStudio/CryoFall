// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Console;

    public class ConsoleAdminGiveawayLP : BaseConsoleCommand
    {
        public override string Description => "Adds specified item(s) to all player characters.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.giveawayLP";

        public string Execute(ushort learningPoints)
        {
            var playersProcessed = 0;
            foreach (var player in Server.Characters.EnumerateAllPlayerCharacters(
                onlyOnline: false,
                exceptSpectators: false))
            {
                var technologies = player.SharedGetTechnologies();
                technologies.ServerSetLearningPoints(technologies.LearningPoints + learningPoints);
                playersProcessed++;
            }

            return $"{learningPoints} LP were added to {playersProcessed} players.";
        }
    }
}