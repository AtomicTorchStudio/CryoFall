namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.World
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConsoleWorldPlaceObject : BaseConsoleCommand
    {
        public override string Alias => "place";

        public override string Description =>
            @"Places a world object in the player character position or in any specified position.
              You can use this to spawn new buildings and resources.
              You need to provide a player character name to place the object in its position.
              Please note that the character must be in spectator mode.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "world.place";

        public string Execute(
            IProtoStaticWorldObject protoStaticWorldObject,
            [CurrentCharacterIfNull] ICharacter character)
        {
            if (!(character.ProtoCharacter is PlayerCharacterSpectator)
                && !Server.Characters.IsSpectator(character))
            {
                throw new Exception("Player character should be in a spectator mode."
                                    + Environment.NewLine
                                    + "Please this command first: /spectator 1 "
                                    + character.Name);
            }

            return SpawnObject(protoStaticWorldObject, character.TilePosition);
        }

        public string Execute(
            IProtoStaticWorldObject objTypeName,
            ushort x,
            ushort y)
        {
            var offset = Server.World.WorldBounds.Offset;
            return SpawnObject(objTypeName,
                               new Vector2Ushort((ushort)(x + offset.X),
                                                 (ushort)(y + offset.Y)));
        }

        private static string SpawnObject(
            IProtoStaticWorldObject objTypeName,
            Vector2Ushort tilePosition)
        {
            if (objTypeName is IProtoObjectLandClaim)
            {
                throw new Exception("Cannot spawn a land claim object!");
            }

            var result = Server.World.CreateStaticWorldObject(objTypeName, tilePosition);
            return result?.ToString() ?? "<cannot spawn there>";
        }
    }
}