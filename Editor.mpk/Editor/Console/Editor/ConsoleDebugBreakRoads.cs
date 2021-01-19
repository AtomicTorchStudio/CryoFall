// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.Editor.Console.Editor
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Props.Road;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ConsoleDebugBreakRoads : BaseConsoleCommand
    {
        public override string Description =>
            "Replace roads with broken roads according to a special position-derived algorithm. Can be run multiple times as further executions will not do any changes to the already replaced roads.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.breakRoads";

        public string Execute()
        {
            var world = Server.World;
            var changesDone = 0;
            var tempCheckQueue = new Queue<Tile>();
            var tempNeighborTiles = new HashSet<Tile>();

            Process(world.GetStaticWorldObjectsOfProto<ObjectPropRoadHorizontal>(),
                    protoBroken1: Api.GetProtoEntity<ObjectPropRoadHorizontal2>(),
                    protoBroken2: Api.GetProtoEntity<ObjectPropRoadHorizontal3>(),
                    protoBroken3: Api.GetProtoEntity<ObjectPropRoadHorizontal4>(),
                    seed: 696159212);

            Process(world.GetStaticWorldObjectsOfProto<ObjectPropRoadVertical>(),
                    protoBroken1: Api.GetProtoEntity<ObjectPropRoadVertical2>(),
                    protoBroken2: Api.GetProtoEntity<ObjectPropRoadVertical3>(),
                    protoBroken3: Api.GetProtoEntity<ObjectPropRoadVertical4>(),
                    seed: 918246523);

            return $"Replaced {changesDone} roads";

            void Process(
                IEnumerable<IStaticWorldObject> enumerationAllObjects,
                IProtoStaticWorldObject protoBroken1,
                IProtoStaticWorldObject protoBroken2,
                IProtoStaticWorldObject protoBroken3,
                uint seed)
            {
                var listAllObjects = enumerationAllObjects.ToList();
                listAllObjects.Shuffle();

                foreach (var worldObject in listAllObjects)
                {
                    switch (PositionalRandom.Get(worldObject.TilePosition,
                                                 minInclusive: 0,
                                                 maxExclusive: 16,
                                                 seed))
                    {
                        // normal road
                        default:
                            continue;

                        // a little damaged road
                        case 10:
                        case 11:
                        case 12:
                            TryReplace(worldObject, protoBroken1);
                            continue;

                        // damaged road
                        case 13:
                        case 14:
                            TryReplace(worldObject, protoBroken2);
                            continue;

                        // severe damaged road
                        case 15:
                            TryReplace(worldObject, protoBroken3);
                            continue;
                    }
                }

                // helper method to replace the object
                void TryReplace(IStaticWorldObject worldObject, IProtoStaticWorldObject replacementProto)
                {
                    tempCheckQueue.Clear();
                    tempNeighborTiles.Clear();
                    foreach (var tile in worldObject.OccupiedTiles)
                    {
                        tempCheckQueue.Enqueue(tile);
                    }

                    CollectNeighborTilesRecursive(tempCheckQueue, tempNeighborTiles, depthRemains: 12);

                    foreach (var neighborTile in tempNeighborTiles)
                    {
                        foreach (var neighborTileStaticObject in neighborTile.StaticObjects)
                        {
                            if (ReferenceEquals(neighborTileStaticObject.ProtoStaticWorldObject,
                                                replacementProto))
                            {
                                // found same proto type in a neighbor tile, don't allow replacement
                                return;
                            }
                        }
                    }

                    world.DestroyObject(worldObject);
                    world.CreateStaticWorldObject(replacementProto, worldObject.TilePosition);
                    changesDone++;
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
};