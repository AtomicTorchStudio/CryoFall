namespace AtomicTorch.CBND.CoreMod.Systems.WorldMapResourceMarks
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class WorldMapResourceMarksSystem : ProtoSystem<WorldMapResourceMarksSystem>
    {
        private static int? clientServerResourceSpawnClaimingCooldownDuration;

        private static ILogicObject serverManagerInstance;

        private static NetworkSyncList<WorldMapResourceMark> sharedResourceMarksList;

        static WorldMapResourceMarksSystem()
        {
            IsResourceDepositCoordinatesHiddenUntilCapturePossible =
                ServerRates.Get(
                    "IsResourceDepositCoordinatesHiddenUntilCapturePossible",
                    defaultValue: 0,
                    @"(for PvP servers only) Set it to 1 to hide the resource deposit (such as oil or Li)
                       world coordinates until the capture is possible.
                       When coordinates are hidden, players will receive only a biome name
                       instead of the actual coordinates for the resource deposit.")
                > 0;
        }

        public static event Action ClientDepositClaimCooldownDurationReceived;

        public static event Action<WorldMapResourceMark> ClientMarkAdded;

        public static event Action<WorldMapResourceMark> ClientMarkRemoved;

        public static bool IsResourceDepositCoordinatesHiddenUntilCapturePossible { get; }

        public override string Name => "World map resource marks system";

        public static void ServerAddMark(IStaticWorldObject staticWorldObject, double serverSpawnTime)
        {
            Api.ValidateIsServer();

            ushort searchAreaCircleRadius = 0;
            var searchAreaCirclePosition = Vector2Ushort.Zero;
            var timeToClaimRemains = SharedCalculateTimeToClaimLimitRemovalSeconds(serverSpawnTime);

            var biome = staticWorldObject.OccupiedTile.ProtoTile;
            var position = SharedGetObjectCenterPosition(staticWorldObject);

            if (IsResourceDepositCoordinatesHiddenUntilCapturePossible
                && timeToClaimRemains > 0)
            {
                searchAreaCircleRadius = 210;
                searchAreaCirclePosition = ServerCalculateSearchAreaApproximateCircle(staticWorldObject,
                                                                                      position,
                                                                                      biome,
                                                                                      searchAreaCircleRadius);
                // hide position
                position = Vector2Ushort.Zero;
            }

            sharedResourceMarksList.Add(
                new WorldMapResourceMark(staticWorldObject.Id,
                                         position,
                                         staticWorldObject.ProtoStaticWorldObject,
                                         serverSpawnTime,
                                         biome: biome,
                                         searchAreaCirclePosition: searchAreaCirclePosition,
                                         searchAreaCircleRadius: searchAreaCircleRadius));

            if (!IsResourceDepositCoordinatesHiddenUntilCapturePossible)
            {
                return;
            }

            if (timeToClaimRemains <= 0)
            {
                return;
            }

            ServerTimersSystem.AddAction(
                timeToClaimRemains + 1,
                () =>
                {
                    if (staticWorldObject.IsDestroyed)
                    {
                        return;
                    }

                    Logger.Important("It's possible to capture the resource deposit now, adding a mark on the map: "
                                     + staticWorldObject);
                    ServerRemoveMark(staticWorldObject);

                    // add on the next frame (give to for the network replication system)
                    ServerTimersSystem.AddAction(
                        0.1,
                        () =>
                        {
                            if (staticWorldObject.IsDestroyed)
                            {
                                return;
                            }

                            ServerAddMark(staticWorldObject, serverSpawnTime);
                        });
                });
        }

        public static void ServerRemoveMark(IStaticWorldObject staticWorldObject)
        {
            Api.ValidateIsServer();
            var id = staticWorldObject.Id;

            // find and remove the mark
            for (var index = 0; index < sharedResourceMarksList.Count; index++)
            {
                var mark = sharedResourceMarksList[index];
                if (mark.Id != id)
                {
                    continue;
                }

                sharedResourceMarksList.RemoveAt(index);
                return;
            }
        }

        public static int SharedCalculateTimeRemainsToClaimCooldownSeconds(WorldMapResourceMark mark)
        {
            return (int)SharedCalculateTimeToClaimLimitRemovalSeconds(mark.ServerSpawnTime);
        }

        public static int SharedCalculateTimeRemainsToClaimCooldownSeconds(IStaticWorldObject staticWorldObject)
        {
            foreach (var mark in sharedResourceMarksList)
            {
                if (mark.Id != staticWorldObject.Id)
                {
                    continue;
                }

                return (int)SharedCalculateTimeToClaimLimitRemovalSeconds(mark.ServerSpawnTime);
            }

            return 0;
        }

        public static double SharedCalculateTimeToClaimLimitRemovalSeconds(double markServerSpawnTime)
        {
            if (markServerSpawnTime <= 0)
            {
                return 0;
            }

            var serverTime = IsClient
                                 ? Api.Client.CurrentGame.ServerFrameTimeApproximated
                                 : Api.Server.Game.FrameTime;

            var timeSinceSpawn = serverTime - markServerSpawnTime;

            int claimCooldownDuration;
            if (Api.IsServer)
            {
                claimCooldownDuration = StructureConstants.DepositsSpawnClaimingCooldownDuration;
            }
            else
            {
                if (!clientServerResourceSpawnClaimingCooldownDuration.HasValue)
                {
                    Logger.Info("No data received yet for " + nameof(clientServerResourceSpawnClaimingCooldownDuration));
                    return 0;
                }

                claimCooldownDuration = clientServerResourceSpawnClaimingCooldownDuration.Value;
            }

            var resultSeconds = claimCooldownDuration - timeSinceSpawn;
            return Math.Max(resultSeconds, 0);
        }

        public static IEnumerable<WorldMapResourceMark> SharedEnumerateMarks()
        {
            if (sharedResourceMarksList == null)
            {
                yield break;
            }

            foreach (var entry in sharedResourceMarksList)
            {
                yield return entry;
            }
        }

        public static Vector2Ushort SharedGetObjectCenterPosition(IStaticWorldObject staticWorldObject)
        {
            var position = staticWorldObject.TilePosition;
            var layoutCenter = staticWorldObject.ProtoStaticWorldObject.Layout.Center.ToVector2Ushort();
            return new Vector2Ushort((ushort)(position.X + layoutCenter.X),
                                     (ushort)(position.Y + layoutCenter.Y));
        }

        public static bool SharedIsContainsMark(in WorldMapResourceMark mark)
        {
            if (sharedResourceMarksList == null)
            {
                return false;
            }

            foreach (var entry in sharedResourceMarksList)
            {
                if (mark.Equals(entry))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Calculates the search area position and radius.
        /// </summary>
        private static Vector2Ushort ServerCalculateSearchAreaApproximateCircle(
            IStaticWorldObject staticWorldObject,
            Vector2Ushort position,
            IProtoTile biome,
            ushort circleRadius)
        {
            var serverWorld = Server.World;
            var biomeSessionIndex = biome.SessionIndex;

            var circleCenter = Vector2Ushort.Zero;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                if (TryToCreateSearchArea(desiredBiomeMatchRatio: 0.75)
                    || TryToCreateSearchArea(desiredBiomeMatchRatio: 0.5)
                    || TryToCreateSearchArea(desiredBiomeMatchRatio: 0.25)
                    || TryToCreateSearchArea(desiredBiomeMatchRatio: 0.1))
                {
                    return circleCenter;
                }

                Logger.Warning(
                    "Unable to calculate an approximate search area for the resource deposit location, will return a random area: "
                    + staticWorldObject);
                return circleCenter;
            }
            finally
            {
                Logger.Important(
                    $"Calculating a resource deposit search area took {stopwatch.ElapsedMilliseconds}ms (for {staticWorldObject} in {biome.ShortId} biome)");
            }

            bool TryToCreateSearchArea(double desiredBiomeMatchRatio)
            {
                for (var attempt = 0; attempt < 100; attempt++)
                {
                    // randomize circle offset to be somewhere within 5-100% from the actual center
                    var offset = circleRadius * (0.05 + 0.95 * RandomHelper.NextDouble());

                    var angle = RandomHelper.NextDouble() * MathConstants.DoublePI;
                    var resultD = new Vector2D(position.X + offset * Math.Cos(angle),
                                               position.Y + offset * Math.Sin(angle));

                    circleCenter = new Vector2Ushort((ushort)MathHelper.Clamp(resultD.X, 0, ushort.MaxValue),
                                                     (ushort)MathHelper.Clamp(resultD.Y, 0, ushort.MaxValue));

                    if (IsValidCircle())
                    {
                        return true;
                    }

                    bool IsValidCircle()
                    {
                        uint totalChecks = 0,
                             biomeMathes = 0,
                             waterOrOutOfBounds = 0;
                        for (var x = -circleRadius; x < circleRadius; x += 10)
                        for (var y = -circleRadius; y < circleRadius; y += 10)
                        {
                            totalChecks++;
                            var tile = serverWorld.GetTile(circleCenter.X + x,
                                                           circleCenter.Y + y,
                                                           logOutOfBounds: false);
                            if (tile.IsOutOfBounds)
                            {
                                waterOrOutOfBounds++;
                                biomeMathes++; // yes, consider it a biome match
                                continue;
                            }

                            if (tile.ProtoTileSessionIndex == biomeSessionIndex)
                            {
                                biomeMathes++;
                            }
                            else if (tile.ProtoTile.Kind == TileKind.Water)
                            {
                                waterOrOutOfBounds++;
                                biomeMathes++; // yes, consider it a biome match
                            }
                        }

                        var biomeMatchRatio = biomeMathes / (double)totalChecks;
                        var waterOrOutOfBoundsRatio = waterOrOutOfBounds / (double)totalChecks;
                        return biomeMatchRatio >= desiredBiomeMatchRatio
                               && waterOrOutOfBoundsRatio <= 0.3;
                    }
                }

                return false;
            }
        }

        private ILogicObject ServerRemote_AcquireManagerInstance()
        {
            var character = ServerRemoteContext.Character;
            Logger.Important("World map resources requested from server");
            Server.World.ForceEnterScope(character, serverManagerInstance);
            return serverManagerInstance;
        }

        private int ServerRemote_AcquireServerResourceSpawnClaimingCooldownDuration()
        {
            return StructureConstants.DepositsSpawnClaimingCooldownDuration;
        }

        [PrepareOrder(afterType: typeof(BootstrapperServerCore))]
        public class BootstrapperWorldMapResourcesSystem : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += ClientTryRequestWorldResourcesAsync;

                ClientTryRequestWorldResourcesAsync();

                async void ClientTryRequestWorldResourcesAsync()
                {
                    if (Api.Client.Characters.CurrentPlayerCharacter == null)
                    {
                        return;
                    }

                    // researched technologies might be still not received so let's wait a bit
                    ClientTimersSystem.AddAction(
                        delaySeconds: 3,
                        async () =>
                        {
                            if (Api.Client.Characters.CurrentPlayerCharacter == null)
                            {
                                return;
                            }

                            Logger.Important("World map resource marks requested from server");

                            var managerInstance =
                                await Instance.CallServer(
                                    _ => _.ServerRemote_AcquireManagerInstance());

                            var marksList = WorldMapResourceMarksManager
                                            .GetPublicState(managerInstance)
                                            .Marks;
                            Logger.Important(
                                $"World map resource marks received from server: {marksList.Count} marks total");

                            if (sharedResourceMarksList != null)
                            {
                                var onRemoved = ClientMarkRemoved;
                                if (onRemoved != null)
                                {
                                    foreach (var mark in
                                        sharedResourceMarksList)
                                    {
                                        onRemoved.Invoke(mark);
                                    }
                                }

                                sharedResourceMarksList.ClientElementInserted
                                    -= this
                                        .ClientMarksListElementInsertedHandler;
                                sharedResourceMarksList.ClientElementRemoved
                                    -= this
                                        .ClientMarksListElementRemovedHandler;
                            }

                            sharedResourceMarksList = marksList;
                            sharedResourceMarksList.ClientElementInserted += this.ClientMarksListElementInsertedHandler;
                            sharedResourceMarksList.ClientElementRemoved += this.ClientMarksListElementRemovedHandler;

                            var onAdded = ClientMarkAdded;
                            if (onAdded != null)
                            {
                                foreach (var mark in sharedResourceMarksList)
                                {
                                    onAdded.Invoke(mark);
                                }
                            }
                        });

                    // TODO: A25 remove the try-catch block here
                    try
                    {
                        clientServerResourceSpawnClaimingCooldownDuration = null;
                        var taskGetCooldownDuration = Instance.CallServer(
                            _ => _.ServerRemote_AcquireServerResourceSpawnClaimingCooldownDuration());
                        await taskGetCooldownDuration;
                        clientServerResourceSpawnClaimingCooldownDuration = taskGetCooldownDuration.Result;
                        Logger.Important("Received deposit claiming cooldown duration: " + clientServerResourceSpawnClaimingCooldownDuration);
                    }
                    catch
                    {
                        clientServerResourceSpawnClaimingCooldownDuration = 30 * 60;
                        Logger.Warning("Not received the deposit claiming cooldown duration, using the legacy value: "
                                   + clientServerResourceSpawnClaimingCooldownDuration);
                    }

                    Api.SafeInvoke(ClientDepositClaimCooldownDurationReceived);
                }
            }

            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                Server.World.WorldBoundsChanged += this.ServerWorldBoundsChangedHandler;

                ServerLoadSystem();
            }

            private static void ServerLoadSystem()
            {
                const string key = nameof(WorldMapResourceMarksManager);
                if (Server.Database.TryGet(key, key, out ILogicObject savedManager))
                {
                    Server.World.DestroyObject(savedManager);
                }

                serverManagerInstance = Server.World.CreateLogicObject<WorldMapResourceMarksManager>();
                Server.Database.Set(key, key, serverManagerInstance);

                var publicState = WorldMapResourceMarksManager.GetPublicState(serverManagerInstance);
                sharedResourceMarksList = new NetworkSyncList<WorldMapResourceMark>();
                publicState.Marks = sharedResourceMarksList;
            }

            private void ClientMarksListElementInsertedHandler(
                NetworkSyncList<WorldMapResourceMark> source,
                int index,
                WorldMapResourceMark value)
            {
                ClientMarkAdded?.Invoke(value);
            }

            private void ClientMarksListElementRemovedHandler(
                NetworkSyncList<WorldMapResourceMark> source,
                int index,
                WorldMapResourceMark removedValue)
            {
                ClientMarkRemoved?.Invoke(removedValue);
            }

            private void ServerWorldBoundsChangedHandler()
            {
                const string key = nameof(WorldMapResourceMarksManager);
                Server.Database.Remove(key, key);
                Server.World.DestroyObject(serverManagerInstance);
                ServerLoadSystem();
            }
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                Logger.Important("World marks system initialized. Deposit marks are "
                                 + (IsResourceDepositCoordinatesHiddenUntilCapturePossible ? "hidden" : "displayed")
                                 + " until it's possible to capture them.");
            }
        }
    }
}