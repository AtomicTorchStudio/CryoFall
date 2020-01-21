namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;
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
        // Please note:
        // usually this method finishes quickly so it's not spread across multiple frames.
        private static void ServerPurgeBuildingsInDestroyedLandClaimArea(
            IStaticWorldObject landClaimStructure,
            in RectangleInt areaBounds)
        {
            var stopwatch = Stopwatch.StartNew();
            var tempObjects = new HashSet<IStaticWorldObject>();
            var serverWorld = Api.Server.World;
            var serverItems = Api.Server.Items;
            var logger = Api.Logger;

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

            var purgedContainersCount = 0;
            foreach (var worldObject in tempObjects)
            {
                switch (worldObject.ProtoStaticWorldObject)
                {
                    case IProtoObjectCrate _ when SharedIsObjectInFreeArea():
                    {
                        var privateState = worldObject.GetPrivateState<ObjectCratePrivateState>();
                        PurgeContainer(privateState.ItemsContainer);
                        break;
                    }

                    case IProtoObjectTradingStation _ when SharedIsObjectInFreeArea():
                    {
                        var privateState = worldObject.GetPrivateState<ObjectTradingStationPrivateState>();
                        PurgeContainer(privateState.StockItemsContainer);
                        break;
                    }

                    case IProtoObjectBarrel _ when SharedIsObjectInFreeArea():
                    {
                        var privateState = worldObject.GetPrivateState<ProtoBarrelPrivateState>();
                        privateState.LiquidAmount = 0;
                        privateState.LiquidType = null;
                        PurgeManufacturerContainers(privateState);
                        break;
                    }

                    case IProtoObjectManufacturer _ when SharedIsObjectInFreeArea():
                    {
                        var privateState = worldObject.GetPrivateState<ObjectManufacturerPrivateState>();
                        PurgeManufacturerContainers(privateState);
                        break;
                    }
                }

                bool SharedIsObjectInFreeArea()
                    => !LandClaimSystem.SharedIsObjectInsideAnyArea(worldObject);
            }

            stopwatch.Stop();
            logger.Important(
                $"Land claim destroyed: {landClaimStructure}. Items containers purged: {purgedContainersCount}. Time spent: {stopwatch.Elapsed.TotalMilliseconds}ms");

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
                RectangleInt areaBounds)
            {
                ServerPurgeBuildingsInDestroyedLandClaimArea(landClaimStructure, areaBounds);
            }
        }
    }
}