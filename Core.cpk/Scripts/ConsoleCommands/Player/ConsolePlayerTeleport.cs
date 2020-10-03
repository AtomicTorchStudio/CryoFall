// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Player
{
    using System;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConsolePlayerTeleport : BaseConsoleCommand
    {
        private static readonly IWorldServerService ServerWorldService = IsServer ? Server.World : null;

        public override string Alias => "tp";

        public override string Description =>
            "Teleports a player character to the specified tile position in the world or to another player depending on arguments used.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerModerator;

        public override string Name => "player.teleport";

        public static Task<string> ClientCallTeleportAsync(Vector2D worldPosition)
        {
            return ConsolePlayerTeleportSystem.Instance
                                              .CallServer(_ => _.ServerRemote_Teleport(worldPosition));
        }

        public string Execute(ushort x, ushort y, [CurrentCharacterIfNull] ICharacter player = null)
        {
            var worldOffset = ServerWorldService.WorldBounds.Offset;
            x += worldOffset.X;
            y += worldOffset.Y;

            ServerTeleport(player, (x, y));
            return CreateResultMessage(player);
        }

        // we have to disable this implementation as the game confuses this execute method with the next one (with toPlayer argument)
        //public string Execute(string coordinate, [CurrentCharacterIfNull] ICharacter player = null)
        //{
        //    var x = ushort.Parse(coordinate.Split(';')[0]);
        //    var y = ushort.Parse(coordinate.Split(';')[1]);

        //    var worldOffset = ServerWorldService.WorldBounds.Offset;
        //    x += worldOffset.X;
        //    y += worldOffset.Y;

        //    ServerTeleport(player, (x, y));
        //    return CreateResultMessage(player);
        //}

        public string Execute(ICharacter toPlayer, [CurrentCharacterIfNull] ICharacter player = null)
        {
            if (toPlayer == player)
            {
                return
                    "Cannot teleport to \"yourself\", you need to specify another character as a location or world coordinates.";
            }

            ServerTeleport(player, toPlayer.Position);
            return CreateResultMessage(player);
        }

        private static string CreateResultMessage(ICharacter character)
        {
            return $"{character} teleported to {character.Position - (Vector2D)ServerWorldService.WorldBounds.Offset}.";
        }

        private static Vector2D FindClosestValidPosition(Vector2D position)
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
                using var objectsNearby = physicsSpace.TestRectangle(
                    // include some padding, otherwise the check will include border-objects
                    position: pos + (0.01, 0.01),
                    size: (0.98, 0.98),
                    collisionGroup: collisionGroup,
                    sendDebugEvent: false);
                return objectsNearby.Count == 0;
            }
        }

        private static void ServerTeleport(ICharacter player, Vector2D toPosition)
        {
            if (player.PhysicsBody.HasAnyShapeCollidingWithGroup(CollisionGroup.GetDefault()))
            {
                // perform position validity check
                toPosition = FindClosestValidPosition(toPosition);
            }
            else
            {
                // This is a character without the physical collider (probably a spectator).
                // It could be teleported anywhere.
            }

            var objectToTeleport = player.SharedGetCurrentVehicle()
                                   ?? player;
            ServerWorldService.SetPosition(objectToTeleport, toPosition);
        }

        private class ConsolePlayerTeleportSystem : ProtoSystem<ConsolePlayerTeleportSystem>
        {
            public override string Name => nameof(ConsolePlayerTeleportSystem);

            [RemoteCallSettings(DeliveryMode.ReliableSequenced, avoidBuffer: true)]
            public string ServerRemote_Teleport(Vector2D worldPosition)
            {
                try
                {
                    var character = ServerRemoteContext.Character;
                    if (character.ProtoCharacter is PlayerCharacterSpectator
                        || ServerOperatorSystem.SharedIsOperator(character)
                        || CreativeModeSystem.SharedIsInCreativeMode(character))
                    {
                        ServerTeleport(character, worldPosition);
                        var message = CreateResultMessage(character);
                        Logger.Important(message);
                        return message;
                    }

                    return
                        "Error: you're not in creative mode and not a spectator. You cannot use the teleport system.";
                }
                catch (Exception ex)
                {
                    var message = ex.Message;
                    Logger.Important(message);
                    return "Error: " + message;
                }
            }
        }
    }
}