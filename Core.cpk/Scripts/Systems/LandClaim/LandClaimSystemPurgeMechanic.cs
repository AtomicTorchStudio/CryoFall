namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
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
        /// <summary>
        /// For PvE servers, the structures inside the decayed land claim areas are removed completely.
        /// </summary>
        private static async Task<int> ServerDeleteAllStructuresAsync(
            HashSet<IStaticWorldObject> staticWorldObjects,
            Func<Task> yieldIfOutOfTime)
        {
            var world = Api.Server.World;
            var objectsDeletedCount = 0;

            var tempDestroyList = new List<IStaticWorldObject>();

            foreach (var worldObject in staticWorldObjects)
            {
                await yieldIfOutOfTime();

                var protoStaticWorldObject = worldObject.ProtoStaticWorldObject;
                if (!(protoStaticWorldObject is IProtoObjectStructure)
                    || LandClaimSystem.SharedIsObjectInsideAnyArea(worldObject))
                {
                    continue;
                }

                world.DestroyObject(worldObject);
                objectsDeletedCount++;

                // destroy any ground containers that appeared in the released space
                foreach (var occupiedTilePosition in worldObject.OccupiedTilePositions)
                {
                    var tile = world.GetTile(occupiedTilePosition);
                    foreach (var staticObjectInPlace in tile.StaticObjects)
                    {
                        if (staticObjectInPlace.ProtoGameObject is ObjectGroundItemsContainer)
                        {
                            tempDestroyList.Add(staticObjectInPlace);
                        }
                    }
                }

                if (tempDestroyList.Count > 0)
                {
                    foreach (var staticObjectInPlace in tempDestroyList)
                    {
                        world.DestroyObject(staticObjectInPlace);
                    }

                    tempDestroyList.Clear();
                }
            }

            return objectsDeletedCount;
        }

        /// <summary>
        /// This method will purge the land in the decayed land claim area.
        /// Please note: there is a small chance that the savegame is made right in the middle of the deletion.
        /// This way when the savegame is loaded the deletion will not resume.
        /// </summary>
        private static async void ServerPurgeStructuresInDestroyedLandClaimArea(
            IStaticWorldObject landClaimStructure,
            RectangleInt areaBounds)
        {
            await Api.Server.Core.AwaitEndOfFrame;

            var logger = Api.Logger;
            var stopwatch = Stopwatch.StartNew();
            var tempObjects = new HashSet<IStaticWorldObject>();
            var serverWorld = Api.Server.World;
            var serverItems = Api.Server.Items;

            var yieldIfOutOfTime = (Func<Task>)Api.Server.Core.YieldIfOutOfTime;

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

                await yieldIfOutOfTime();
            }

            var objectsDeletedCount = 0;
            var purgedContainersCount = 0;

            foreach (var worldObject in tempObjects)
            {
                await yieldIfOutOfTime();

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
                objectsDeletedCount = await ServerDeleteAllStructuresAsync(tempObjects, yieldIfOutOfTime);
            }

            stopwatch.Stop();
            logger.Dev(
                $"Land claim destroyed: {landClaimStructure}. Objects deleted: {objectsDeletedCount}. Item containers purged: {purgedContainersCount}. Time spent: {stopwatch.Elapsed.TotalMilliseconds}ms (spread across multiple frames)");

            void PurgeContainer(IItemsContainer container)
            {
                if (container is null
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
                LandClaimAreaPublicState areaPublicState,
                bool isDestroyedByPlayers,
                bool isDeconstructed)
            {
                if (isDeconstructed)
                {
                    return;
                }

                if (!PveSystem.ServerIsPvE
                    && isDestroyedByPlayers)
                {
                    Api.Logger.Important(
                        $"Land claim destroyed after timer due to players' attack: {landClaimStructure}. No items were purged.");
                    return;
                }

                ServerPurgeStructuresInDestroyedLandClaimArea(landClaimStructure, areaBounds);
            }
        }
    }
}