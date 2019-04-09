// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.WorldDiscovery;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsolePlayerDiscoverMap : BaseConsoleCommand
    {
        public override string Description => "Discover whole map for the player character.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.discoverMap";

        public string Execute([CurrentCharacterIfNull] ICharacter player)
        {
            WorldDiscoverySystem.Instance.ServerDiscoverWorldChunks(
                player,
                Server.World.GetAllChunkTilePositions());

            return $"{player} now discovered whole map.";
        }
    }
}