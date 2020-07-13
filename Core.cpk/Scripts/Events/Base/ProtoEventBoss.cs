namespace AtomicTorch.CBND.CoreMod.Events.Base
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
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

        public override bool ConsolidateNotifications => false;

        public override double ServerUpdateIntervalSeconds => 1;

        public IReadOnlyList<IProtoSpawnableObject> SpawnPreset { get; private set; }

        public override string SharedGetProgressText(ILogicObject activeEvent)
        {
            return null;
        }

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

        protected abstract void ServerPrepareBossEvent(Triggers triggers, List<IProtoSpawnableObject> spawnPreset);

        protected sealed override void ServerPrepareEvent(Triggers triggers)
        {
            var list = new List<IProtoSpawnableObject>();
            this.ServerPrepareBossEvent(triggers, list);
            Api.Assert(list.Count > 0, "Spawn preset cannot be empty");
            this.SpawnPreset = list;
        }

        protected abstract Vector2Ushort ServerSelectSpawnPosition(Vector2Ushort circlePosition, ushort circleRadius);

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

            foreach (var spawnedObject in GetPrivateState(activeEvent).SpawnedWorldObjects)
            {
                if (spawnedObject.IsDestroyed)
                {
                    continue;
                }

                if (!Server.World.IsObservedByAnyPlayer(spawnedObject))
                {
                    Server.World.DestroyObject(spawnedObject);
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