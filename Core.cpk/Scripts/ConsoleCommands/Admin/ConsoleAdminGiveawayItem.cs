// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ConsoleAdminGiveawayItem : BaseConsoleCommand
    {
        public override string Description => "Adds specified item(s) to all player characters.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.giveawayItem";

        public string Execute(IProtoItem item, ushort count = 1)
        {
            var playersProcessed = 0;
            foreach (var player in Server.Characters.EnumerateAllPlayerCharacters(
                onlyOnline: false,
                exceptSpectators: false))
            {
                Server.Items.CreateItem(item, player, count);
                playersProcessed++;
            }

            return $"{item.Name} (x{count}) were added to {playersProcessed} players.";
        }
    }
}