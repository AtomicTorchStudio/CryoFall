// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.World
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConsoleWorldDestroyObject : BaseConsoleCommand
    {
        public override string Description =>
            @"Destroys the closest world object in the player character position or neighbor tiles.
              You can use this to destroy any creatures, buildings or objects such as resources.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "world.destroy";

        public string Execute([CurrentCharacterIfNull] ICharacter character)
        {
            return DestroyFirstObject(character.Tile);
        }

        public string Execute(ushort x, ushort y)
        {
            var offset = Server.World.WorldBounds.Offset;
            var tilePosition = new Vector2Ushort((ushort)(x + offset.X),
                                                 (ushort)(y + offset.Y));

            var tile = Server.World.GetTile(tilePosition);
            return DestroyFirstObject(tile);
        }

        private static string DestroyFirstObject(Tile tile)
        {
            ICharacter characterNpcToDestroy = null;
            foreach (var dynamicObject in tile.EightNeighborTiles.ConcatOne(tile).Reverse()
                                              .SelectMany(t => t.DynamicObjects))
            {
                if (dynamicObject is ICharacter character
                    && character.IsNpc)
                {
                    // found a mob
                    characterNpcToDestroy = character;
                }
            }

            if (characterNpcToDestroy != null)
            {
                Server.World.DestroyObject(characterNpcToDestroy);
                return characterNpcToDestroy + " destroyed";
            }

            var staticWorldObjectToDestroy = tile.StaticObjects.LastOrDefault();
            if (staticWorldObjectToDestroy == null)
            {
                foreach (var neighborTile in tile.EightNeighborTiles)
                {
                    staticWorldObjectToDestroy = neighborTile.StaticObjects.LastOrDefault();
                    if (staticWorldObjectToDestroy != null)
                    {
                        break;
                    }
                }
            }

            if (staticWorldObjectToDestroy == null)
            {
                return "No world objects nearby to destroy";
            }

            Server.World.DestroyObject(staticWorldObjectToDestroy);
            return staticWorldObjectToDestroy + " destroyed";
        }
    }
}