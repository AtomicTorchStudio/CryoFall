// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.Editor.Console.Editor
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Props.Walls;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ConsoleDebugBreakRuinWalls : BaseConsoleCommand
    {
        public override string Description =>
            "Replace ruin/radtown walls with broken wall according to a special position-derived algorithm. Can be run multiple times as further executions will not do any changes to the already replaced walls.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "editor.breakRuinWalls";

        public string Execute()
        {
            var world = Server.World;
            var changesDone = 0;
            var tempCheckQueue = new Queue<Tile>();
            var tempNeighborTiles = new HashSet<Tile>();

            Process(world.GetStaticWorldObjectsOfProto<ObjectPropRuinsWallHorizontal01>(),
                    protoBroken1: Api.GetProtoEntity<ObjectPropRuinsWallHorizontal02>(),
                    protoBroken2: Api.GetProtoEntity<ObjectPropRuinsWallHorizontal03>(),
                    protoBroken3: Api.GetProtoEntity<ObjectPropRuinsWallHorizontal04>(),
                    seed: 823731251,
                    defaultVariantRateIncrease: 4);

            Process(world.GetStaticWorldObjectsOfProto<ObjectPropRuinsWallVertical01>(),
                    protoBroken1: Api.GetProtoEntity<ObjectPropRuinsWallVertical01>(),
                    protoBroken2: Api.GetProtoEntity<ObjectPropRuinsWallVertical02>(),
                    protoBroken3: Api.GetProtoEntity<ObjectPropRuinsWallVertical02>(),
                    seed: 155436192,
                    defaultVariantRateIncrease: 2);

            return $"Replaced {changesDone} walls";

            void Process(
                IEnumerable<IStaticWorldObject> enumerationAllObjects,
                IProtoStaticWorldObject protoBroken1,
                IProtoStaticWorldObject protoBroken2,
                IProtoStaticWorldObject protoBroken3,
                uint seed,
                byte defaultVariantRateIncrease)
            {
                var listAllObjects = enumerationAllObjects.ToList();
                listAllObjects.Shuffle();

                foreach (var worldObject in listAllObjects)
                {
                    bool isOk;
                    var attempt = 0;

                    do
                    {
                        var randomNumber = PositionalRandom.Get(worldObject.TilePosition,
                                                                minInclusive: 0,
                                                                maxExclusive: 4 + defaultVariantRateIncrease,
                                                                seed);
                        isOk = randomNumber switch
                        {
                            0 => TryReplace(worldObject, protoBroken1),
                            1 => TryReplace(worldObject, protoBroken2),
                            2 => TryReplace(worldObject, protoBroken3),
                            _ => true
                        };
                    }
                    while (!isOk
                           && ++attempt < 8);
                }

                // helper method to replace the object
                bool TryReplace(IStaticWorldObject worldObject, IProtoStaticWorldObject replacementProto)
                {
                    if (ReferenceEquals(worldObject.ProtoGameObject, replacementProto))
                    {
                        // no replacement necessary
                        return true;
                    }

                    tempCheckQueue.Clear();
                    tempNeighborTiles.Clear();
                    foreach (var tile in worldObject.OccupiedTiles)
                    {
                        tempCheckQueue.Enqueue(tile);
                    }

                    CollectNeighborTilesRecursive(tempCheckQueue,
                                                  tempNeighborTiles,
                                                  // will process only up to 4 neighbors
                                                  // by distance from the occupied tiles
                                                  depthRemains: 5);

                    foreach (var neighborTile in tempNeighborTiles)
                    {
                        foreach (var neighborTileStaticObject in neighborTile.StaticObjects)
                        {
                            if (ReferenceEquals(neighborTileStaticObject.ProtoStaticWorldObject,
                                                replacementProto))
                            {
                                // found the same proto type in a neighbor tile, don't allow replacement
                                return false;
                            }
                        }
                    }

                    world.DestroyObject(worldObject);
                    world.CreateStaticWorldObject(replacementProto, worldObject.TilePosition);
                    changesDone++;
                    return true;
                }
            }
        }

        private static void CollectNeighborTilesRecursive(
            Queue<Tile> tempCheckQueue,
            HashSet<Tile> tempAllNeighborTiles,
            int depthRemains)
        {
            var nextDepthRemains = depthRemains - 1;

            var count = tempCheckQueue.Count;
            for (var i = 0; i < count; i++)
            {
                var tile = tempCheckQueue.Dequeue();
                if (!tempAllNeighborTiles.AddIfNotContains(tile))
                {
                    continue;
                }

                if (nextDepthRemains == 0)
                {
                    continue;
                }

                foreach (var otherTile in tile.EightNeighborTiles)
                {
                    if (otherTile.IsValidTile)
                    {
                        tempCheckQueue.Enqueue(otherTile);
                    }
                }
            }

            if (nextDepthRemains == 0)
            {
                return;
            }

            CollectNeighborTilesRecursive(tempCheckQueue, tempAllNeighborTiles, nextDepthRemains);
        }
    }
}