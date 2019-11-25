namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.World
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConsoleWorldSpawnVehicle : BaseConsoleCommand
    {
        public override string Alias => "spawnVehicle";

        public override string Description =>
            @"Spawns a vehicle in the player character position or in any specified position.
              You need to provide a player character name to place a vehicle in its position.
              Please note that the character must be in spectator mode.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "world.spawnVehicle";

        public string Execute(
            IProtoVehicle protoDynamicWorldObject,
            [CurrentCharacterIfNull] ICharacter character)
        {
            return SpawnObject(protoDynamicWorldObject, character.TilePosition.ToVector2D());
        }

        public string Execute(
            IProtoVehicle objTypeName,
            double x,
            double y)
        {
            var offset = Server.World.WorldBounds.Offset;
            return SpawnObject(objTypeName,
                               (x + offset.X, y + offset.Y));
        }

        private static string SpawnObject(
            IProtoVehicle objTypeName,
            Vector2D position)
        {
            var result = Server.World.CreateDynamicWorldObject(objTypeName, position);
            if (result is null)
            {
                return "<cannot spawn there>";
            }

            var protoVehicle = (IProtoVehicle)result.ProtoGameObject;
            var byCharacter = ServerRemoteContext.IsRemoteCall
                                  ? ServerRemoteContext.Character
                                  : null;
            if (byCharacter != null)
            {
                protoVehicle.ServerOnBuilt(result, byCharacter);
            }

            return result.ToString();
        }
    }
}