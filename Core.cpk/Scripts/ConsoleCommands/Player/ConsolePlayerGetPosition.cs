// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerGetPosition : BaseConsoleCommand
    {
        public override string Alias => "pos";

        public override string Description => "Get the world position of a player character.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.getPosition";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var playerTilePosition = player.TilePosition;
            playerTilePosition -= Server.World.WorldBounds.Offset;
            return $"{player} world position is {playerTilePosition}.";
        }
    }
}