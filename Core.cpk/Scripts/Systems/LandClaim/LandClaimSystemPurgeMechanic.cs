namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    /// <summary>
    /// This mechanic will purge all the containers inside the destroyed land claim area.
    /// </summary>
    [UsedImplicitly]
    public static class LandClaimSystemPurgeMechanic
    {
        private static void ServerDeleteAllStructures(
            HashSet<IStaticWorldObject> staticWorldObjects,
            out int objectsDeletedCount)
        {
            var world = Api.Server.World;
            objectsDeletedCount = 0;

            foreach (var worldObject in staticWorldObjects)
            {
                var protoStaticWorldObject = worldObject.ProtoStaticWorldObject;
                if (!(protoStaticWorldObject is IProtoObjectStructure)
                    || LandClaimSystem.SharedIsObjectInsideAnyArea(worldObject))
                {
                    continue;
                }

                world.DestroyObject(worldObject);
                objectsDeletedCount++;
            }
        }

        // Please note:
        // usually this method finishes quickly so it's not spread across multiple frames.
        private static void ServerPurgeStructuresInDestroyedLandClaimArea(
            IStaticWorldObject landClaimStructure,
            in RectangleInt areaBounds)
        {
            var logger = Api.Logger;
            var stopwatch = Stopwatch.StartNew();
            var tempObjects = new HashSet<IStaticWorldObject>();
            var serverWorld = Api.Server.World;
            var serverItems = Api.Server.Items;

            for (var x = areaBounds.X; x < areaBounds.X + areaBounds.Width; x++)
            for (var y = areaBounds.Y; y < areaBounds.Y + areaBounds.Height; y++)
            {
                if (x < 0
                    || y < 0
                    || x >= ushort.MaxValue
                    || y >= ushort.MaxValue)
                {
                    continue;
                }

                var staticObjects = serverWorld.GetStaticObjects(new Vector2Ushort((ushort)x, (ushort)y));
                foreach (var worldObject in staticObjects)
                {
                    tempObjects.Add(worldObject);
                }
            }

            var objectsDeletedCount = 0;
            var purgedContainersCount = 0;

            foreach (var worldObject in tempObjects)
            {
                var protoStaticWorldObject = worldObject.ProtoStaticWorldObject;
                if (!(protoStaticWorldObject is IProtoObjectStructure)
                    || LandClaimSystem.SharedIsObjectInsideAnyArea(worldObject))
                {
                    continue;
                }

                switch (protoStaticWorldObject)
                {
                    case IProtoObjectCrate _:
                    {
                        var privateState = worldObject.GetPrivateState<ObjectCratePrivateState>();
                        PurgeContainer(privateState.ItemsContainer);
                        break;
                    }

                    case IProtoObjectTradingStation _:
                    {
                        var privateState = worldObject.GetPrivateState<ObjectTradingStationPrivateState>();
                        PurgeContainer(privateState.StockItemsContainer);
                        break;
                    }

                    case IProtoObjectBarrel _:
                    {
                        var privateState = worldObject.GetPrivateState<ProtoBarrelPrivateState>();
                        privateState.LiquidAmount = 0;
                        privateState.LiquidType = null;
                        PurgeManufacturerContainers(privateState);
                        break;
                    }

                    case IProtoObjectManufacturer _:
                    {
                        var privateState = worldObject.GetPrivateState<ObjectManufacturerPrivateState>();
                        PurgeManufacturerContainers(privateState);
                        break;
                    }
                }
            }

            if (PveSystem.ServerIsPvE)
            {
                // on PvE server, delete all the structures within the decayed land claim area
                ServerDeleteAllStructures(tempObjects, out objectsDeletedCount);
            }

            stopwatch.Stop();
            logger.Important(
                $"Land claim destroyed: {landClaimStructure}. Objects deleted: {objectsDeletedCount}. Items containers purged: {purgedContainersCount}. Time spent: {stopwatch.Elapsed.TotalMilliseconds}ms");

            void PurgeContainer(IItemsContainer container)
            {
                if (container == null
                    || container.OccupiedSlotsCount == 0)
                {
                    return;
                }

                logger.Important("Purging items container in the destroyed land claim area: " + container);
                using var items = Api.Shared.WrapInTempList(container.Items);
                foreach (var item in items.AsList())
                {
                    serverItems.DestroyItem(item);
                }

                purgedContainersCount++;
            }

            void PurgeManufacturerContainers(ObjectManufacturerPrivateState privateState)
            {
                PurgeContainer(privateState.ManufacturingState?.ContainerInput);
                PurgeContainer(privateState.ManufacturingState?.ContainerOutput);
                PurgeContainer(privateState.FuelBurningState?.ContainerFuel);
            }
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                base.ServerInitialize(serverConfiguration);
                LandClaimSystem.ServerObjectLandClaimDestroyed += ServerObjectLandClaimDestroyedHandler;
            }

            private static void ServerObjectLandClaimDestroyedHandler(
                IStaticWorldObject landClaimStructure,
                RectangleInt areaBounds,
                bool isDestroyedByPlayers)
            {
                if (!PveSystem.ServerIsPvE
                    && isDestroyedByPlayers)
                {
                    Api.Logger.Important(
                        $"Land claim destroyed after timer due to players' attack: {landClaimStructure}. No items were purged.");
                    return;
                }

                ServerPurgeStructuresInDestroyedLandClaimArea(landClaimStructure,
                                                              areaBounds);
            }
        }
    }
}