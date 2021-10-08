namespace AtomicTorch.CBND.CoreMod.Systems.WorldMapResourceMarks
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class WorldMapResourceMarksSystem : ProtoSystem<WorldMapResourceMarksSystem>
    {
        private const int DepositSearchAreaCircleRadius = 130;

        private static ILogicObject serverManagerInstance;

        private static NetworkSyncList<WorldMapResourceMark> sharedResourceMarksList;

        public static event Action<WorldMapResourceMark> ClientMarkAdded;

        public static event Action<WorldMapResourceMark> ClientMarkRemoved;

        public static void ServerAddMark(IStaticWorldObject staticWorldObject, double serverSpawnTime)
        {
            Api.ValidateIsServer();

            ushort searchAreaCircleRadius = 0;
            var searchAreaCirclePosition = Vector2Ushort.Zero;
            var timeToClaimRemains = SharedCalculateTimeToClaimLimitRemovalSeconds(serverSpawnTime);

            var biome = staticWorldObject.OccupiedTile.ProtoTile;
            var position = SharedGetObjectCenterPosition(staticWorldObject);

            if (RatePvPIsResourceDepositCoordinatesHidden.SharedValue
                && timeToClaimRemains > 0)
            {
                var stopwatch = Stopwatch.StartNew();
                searchAreaCircleRadius = DepositSearchAreaCircleRadius;

                try
                {
                    if (!ServerSearchAreaHelper.GenerateSearchArea(position,
                                                                   biome,
                                                                   searchAreaCircleRadius,
                                                                   out searchAreaCirclePosition,
                                                                   maxAttempts: 100))
                    {
                        Logger.Warning(
                            "Unable to calculate an approximate search area for the resource deposit location, will use the center area: "
                            + staticWorldObject);

                        searchAreaCirclePosition = position;
                    }
                }
                finally
                {
                    Logger.Important(
                        $"Calculating a resource deposit search area took {stopwatch.ElapsedMilliseconds}ms (for {staticWorldObject} in {biome.ShortId} biome)");
                }

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

            if (!RatePvPIsResourceDepositCoordinatesHidden.SharedValue)
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

            var claimDelay = RateResourcesPvPDepositClaimDelay.SharedValue;
            var resultSeconds = claimDelay - timeSinceSpawn;
            return Math.Max(resultSeconds, 0);
        }

        public static IEnumerable<WorldMapResourceMark> SharedEnumerateMarks()
        {
            if (sharedResourceMarksList is null)
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
            if (sharedResourceMarksList is null)
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

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            Logger.Important("World marks system initialized. Deposit marks are "
                             + (RatePvPIsResourceDepositCoordinatesHidden.SharedValue
                                    ? "hidden"
                                    : "displayed")
                             + " until it's possible to capture them.");
        }

        private ILogicObject ServerRemote_AcquireManagerInstance()
        {
            Logger.Info("World map resources requested from server");
            Server.World.ForceEnterScope(ServerRemoteContext.Character, serverManagerInstance);
            return serverManagerInstance;
        }

        [PrepareOrder(afterType: typeof(BootstrapperServerCore))]
        public class Bootstrapper : BaseBootstrapper
        {
            private static int clientLastRequestId;

            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += ClientTryRequestWorldResourcesAsync;

                ClientTryRequestWorldResourcesAsync();

                async void ClientTryRequestWorldResourcesAsync()
                {
                    var requestId = ++clientLastRequestId;
                    if (Api.Client.Characters.CurrentPlayerCharacter is null)
                    {
                        return;
                    }

                    // researched technologies may be still not received so let's wait a bit
                    ClientTimersSystem.AddAction(
                        delaySeconds: 3,
                        async () =>
                        {
                            if (requestId != clientLastRequestId
                                || Api.Client.Characters.CurrentPlayerCharacter is null)
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

                            if (sharedResourceMarksList is not null)
                            {
                                var onRemoved = ClientMarkRemoved;
                                if (onRemoved is not null)
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
                            if (onAdded is not null)
                            {
                                foreach (var mark in sharedResourceMarksList)
                                {
                                    onAdded.Invoke(mark);
                                }
                            }
                        });
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
    }
}