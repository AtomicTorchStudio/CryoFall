// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Mobs
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConsoleMobsSpawn : BaseConsoleCommand
    {
        private static readonly IWorldServerService ServerWorldService = IsServer ? Server.World : null;

        public override string Alias => "spawn";

        public override string Description =>
            "Spawns a mob of the specified type to the specified tile position in the world or near the specified character, depending on arguments used.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "mobs.spawn";

        public string Execute(IProtoCharacterMob protoMob, ushort x, ushort y)
        {
            var worldOffset = ServerWorldService.WorldBounds.Offset;
            x += worldOffset.X;
            y += worldOffset.Y;

            return ServerSpawn(protoMob, (x, y));
        }

        public string Execute(IProtoCharacterMob protoMob, [CurrentCharacterIfNull] ICharacter nearPlayer = null)
        {
            return ServerSpawn(protoMob, nearPlayer.Position);
        }

        private static Vector2D FindClosestPosition(Vector2D position)
        {
            var physicsSpace = ServerWorldService.GetPhysicsSpace();
            var collisionGroup = CollisionGroup.GetDefault();

            if (IsValidPosition(
                // round position
                position.ToVector2Ushort().ToVector2D()))
            {
                // can teleport right there
                return position;
            }

            // check nearby tiles
            var tile = ServerWorldService.GetTile(position.ToVector2Ushort());
            foreach (var neighborTile in tile.EightNeighborTiles)
            {
                if (!neighborTile.IsValidTile)
                {
                    continue;
                }

                var neighborTilePosition = neighborTile.Position.ToVector2D();
                if (IsValidPosition(neighborTilePosition))
                {
                    return neighborTilePosition;
                }
            }

            throw new Exception("No empty position available nearby.");

            // Local function for checking if the position is valid.
            bool IsValidPosition(Vector2D pos)
            {
                using (var objectsNearby = physicsSpace.TestRectangle(
                    // include some padding, otherwise the check will include border-objects
                    position: pos + (0.01, 0.01),
                    size: (0.98, 0.98),
                    collisionGroup: collisionGroup,
                    sendDebugEvent: false))
                {
                    return objectsNearby.Count == 0;
                }
            }
        }

        private static string ServerSpawn(IProtoCharacterMob protoMob, Vector2D position)
        {
            position = FindClosestPosition(position);

            var character = Server.Characters.SpawnCharacter(
                protoMob,
                position);

            if (character == null)
            {
                throw new Exception("Cannot spawn character.");
            }

            return $"{character} spawned at {position}.";
        }
    }
}