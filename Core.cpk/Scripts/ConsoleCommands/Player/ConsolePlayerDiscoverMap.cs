// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.TeleportsSystem;
    using AtomicTorch.CBND.CoreMod.Systems.WorldDiscovery;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConsolePlayerDiscoverMap : BaseConsoleCommand
    {
        public override string Description => "Discover whole map for the player character.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "player.discoverMap";

        public string Execute([CurrentCharacterIfNull] ICharacter player)
        {
            WorldDiscoverySystem.Instance.ServerDiscoverWorldChunks(
                player,
                new List<Vector2Ushort>(Server.World.GetAllChunkTilePositions()));

            TeleportsSystem.ServerDiscoverAllTeleports(player);

            return $"{player} now discovered the whole map and teleports.";
        }
    }
}