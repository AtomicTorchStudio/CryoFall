// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ConsolePlayerItemsAdd : BaseConsoleCommand
    {
        public override string Alias => "addItem";

        public override string Description => "Adds specified item(s) to a player character.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.items.add";

        public string Execute(IProtoItem item, ushort count = 1, [CurrentCharacterIfNull] ICharacter player = null)
        {
            var createItemResult = Server.Items.CreateItem(player, item, count);
            return $"{createItemResult.TotalCreatedCount} item(s) of type {item.Name} added to {player}.";
        }
    }
}