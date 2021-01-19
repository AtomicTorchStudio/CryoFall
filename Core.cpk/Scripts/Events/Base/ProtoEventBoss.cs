namespace AtomicTorch.CBND.CoreMod.Events
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoEventBoss
        : ProtoEventWithArea<
            EventBossPrivateState,
            EventWithAreaPublicState,
            EmptyClientState>
    {
        // {0} is the boss name
        public const string Notification_VictoryAnnouncement_Format =
            "{0} has been defeated by:";

        protected ProtoEventBoss()
        {
            this.ServerSpawnZones = new Lazy<IReadOnlyList<IServerZone>>(
                () =>
                {
                    Api.ValidateIsServer();
                    return this.ServerSetupSpawnZones();
                });
        }

        public override ushort AreaRadius => 55;

        public override bool ConsolidateNotifications => false;

        public IReadOnlyList<IProtoSpawnableObject> SpawnPreset { get; private set; }

        protected Lazy<IReadOnlyList<IServerZone>> ServerSpawnZones { get; }

        public sealed override bool ServerIsTriggerAllowed(ProtoTrigger trigger)
        {
            if (this.ServerIsSameEventExist())
            {
                Logger.Error("The same event is already running, cannot start a new one: " + this);
                return false;
            }

            if (this.ServerSpawnZones.Value.All(z => z.IsEmpty))
            {
                Logger.Error("All zones are empty (not mapped in the world), no place to start the event: " + this);
                return false;
            }

            return this.ServerIsTriggerAllowedForBossEvent(trigger);
        }

        public abstract bool ServerIsTriggerAllowedForBossEvent(ProtoTrigger trigger);

        protected override bool ServerCreateEventSearchArea(
            IWorldServerService world,
            Vector2Ushort eventPosition,
            ushort circleRadius,
            out Vector2Ushort circlePosition)
        {
            circlePosition = eventPosition;
            return true;
        }

        protected override void ServerInitializeEvent(ServerInitializeData data)
        {
            data.PrivateState.Init();
        }

        protected override bool ServerIsValidEventPosition(Vector2Ushort tilePosition)
        {
            return true;
        }

        protected virtual bool ServerIsValidSpawnPosition(Vector2Ushort spawnPosition)
        {
            return true;
        }

        protected virtual void ServerOnBossEventStarted(ILogicObject activeEvent)
        {
        }

        protected override void ServerOnEventDestroyed(ILogicObject activeEvent)
        {
            // destroy all the spawned objects
            foreach (var spawnedObject in GetPrivateState(activeEvent).SpawnedWorldObjects)
            {
                if (!spawnedObject.IsDestroyed)
                {
                    Server.World.DestroyObject(spawnedObject);
                }
            }
        }

        protected sealed override void ServerOnEventWithAreaStarted(ILogicObject activeEvent)
        {
            var privateState = GetPrivateState(activeEvent);
            var publicState = GetPublicState(activeEvent);

            this.ServerSpawnObjects(activeEvent,
                                    publicState.AreaCirclePosition,
                                    publicState.AreaCircleRadius,
                                    privateState.SpawnedWorldObjects);

            this.ServerOnBossEventStarted(activeEvent);
        }

        protected override Vector2Ushort ServerPickEventPosition(ILogicObject activeEvent)
        {
            var stopwatch = Stopwatch.StartNew();

            // select a random boss spawn zone
            var zoneInstance = this.ServerSpawnZones.Value.TakeByRandom();

            // select a random position inside the selected zone
            var randomPosition = zoneInstance.GetRandomPosition(RandomHelper.Instance);

            // use fill flood to locate all the positions
            // within the continuous area around the selected random position
            var chunkPositions = new HashSet<Vector2Ushort>(capacity: 50 * 50);
            var positionToCheck = new Stack<Vector2Ushort>();
            positionToCheck.Push(randomPosition);
            FillFlood();

            // calculate the center position of the area
            Vector2Ushort result = ((ushort)chunkPositions.Average(c => c.X),
                                    (ushort)chunkPositions.Average(c => c.Y));

            Logger.Important(
                $"[Stopwatch] Selecting the boss event position took: {stopwatch.Elapsed.TotalMilliseconds:F1} ms");
            return result;

            void FillFlood()
            {
                while (positionToCheck.Count > 0)
                {
                    var pos = positionToCheck.Pop();
                    if (pos.X == 0
                        || pos.Y == 0
                        || pos.X >= ushort.MaxValue
                        || pos.Y >= ushort.MaxValue)
                    {
                        // reached the bound - not enclosed
                        continue;
                    }

                    if (!chunkPositions.Add(pos))
                    {
                        //  already visited
                        continue;
                    }

                    if (!zoneInstance.IsContainsPosition(pos))
                    {
                        continue;
                    }

                    positionToCheck.Push(((ushort)(pos.X - 1), pos.Y));
                    positionToCheck.Push(((ushort)(pos.X + 1), pos.Y));
                    positionToCheck.Push((pos.X, (ushort)(pos.Y - 1)));
                    positionToCheck.Push((pos.X, (ushort)(pos.Y + 1)));
                }
            }
        }

        protected abstract void ServerPrepareBossEvent(Triggers triggers, List<IProtoSpawnableObject> spawnPreset);

        protected sealed override void ServerPrepareEvent(Triggers triggers)
        {
            var list = new List<IProtoSpawnableObject>();
            this.ServerPrepareBossEvent(triggers, list);
            Api.Assert(list.Count > 0, "Spawn preset cannot be empty");
            this.SpawnPreset = list;
        }

        protected virtual Vector2Ushort ServerSelectSpawnPosition(Vector2Ushort circlePosition, ushort circleRadius)
        {
            return circlePosition;
        }

        protected virtual IReadOnlyList<IServerZone> ServerSetupSpawnZones()
        {
            var result = new List<IServerZone>();
            AddZone(Api.GetProtoEntity<ZoneEventBoss>());

            void AddZone(IProtoZone zone)
            {
                var instance = zone.ServerZoneInstance;
                result.Add(instance);
            }

            return result;
        }

        protected virtual void ServerSpawnObjects(
            ILogicObject activeEvent,
            Vector2Ushort circlePosition,
            ushort circleRadius,
            List<IWorldObject> spawnedObjects)
        {
            foreach (var protoObjectToSpawn in this.SpawnPreset)
            {
                TrySpawn();

                void TrySpawn()
                {
                    var attempts = 1_000;

                    do
                    {
                        // select random position inside the circle
                        var spawnPosition = this.ServerSelectSpawnPosition(circlePosition, circleRadius);

                        if (!this.ServerIsValidSpawnPosition(spawnPosition))
                        {
                            // doesn't match any specific checks determined by the inheritor (such as a zone test)
                            continue;
                        }

                        var spawnedObject = Server.Characters.SpawnCharacter((IProtoCharacter)protoObjectToSpawn,
                                                                             spawnPosition.ToVector2D());
                        spawnedObjects.Add(spawnedObject);
                        Logger.Important($"Spawned world object: {spawnedObject} for active event {activeEvent}");
                        break;
                    }
                    while (--attempts > 0);

                    if (attempts == 0)
                    {
                        Logger.Error($"Cannot spawn world object: {protoObjectToSpawn} for active event {activeEvent}");
                    }
                }
            }
        }

        protected override void ServerTryFinishEvent(ILogicObject activeEvent)
        {
            var canFinish = true;

            var spawnedWorldObjects = GetPrivateState(activeEvent).SpawnedWorldObjects;
            var list = spawnedWorldObjects;
            for (var index = list.Count - 1; index >= 0; index--)
            {
                var spawnedObject = list[index];
                if (spawnedObject.IsDestroyed)
                {
                    spawnedWorldObjects.RemoveAt(index);
                    continue;
                }

                if (!Server.World.IsObservedByAnyPlayer(spawnedObject))
                {
                    Server.World.DestroyObject(spawnedObject);
                    spawnedWorldObjects.RemoveAt(index);
                    continue;
                }

                // still has a spawned object which cannot be destroyed as it's observed by a player
                canFinish = false;
                break;
            }

            if (canFinish)
            {
                base.ServerTryFinishEvent(activeEvent);
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            var activeEvent = data.GameObject;
            var privateState = GetPrivateState(activeEvent);

            var countDestroyed = 0;
            var totalCount = privateState.SpawnedWorldObjects.Count;

            foreach (var spawnedObject in privateState.SpawnedWorldObjects)
            {
                if (spawnedObject.IsDestroyed)
                {
                    countDestroyed++;
                }
            }

            if (countDestroyed == totalCount)
            {
                this.ServerTryFinishEvent(activeEvent);
            }
        }
    }
}