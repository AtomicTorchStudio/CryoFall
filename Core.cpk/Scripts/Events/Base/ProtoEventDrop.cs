namespace AtomicTorch.CBND.CoreMod.Events
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoEventDrop
        : ProtoEventWithArea<
            EventDropPrivateState,
            EventDropPublicState,
            EmptyClientState>
    {
        public const string ProgressTextFormat = "Progress: {0}/{1}";

        public override bool ConsolidateNotifications => true;

        public abstract double MinDistanceBetweenSpawnedObjects { get; }

        public IReadOnlyList<IProtoWorldObject> SpawnPreset { get; private set; }

        public override string SharedGetProgressText(ILogicObject worldEvent)
        {
            var publicState = GetPublicState(worldEvent);
            if (!publicState.IsSpawnCompleted)
            {
                return null;
            }

            return string.Format(ProgressTextFormat,
                                 publicState.ObjectsTotal - publicState.ObjectsRemains,
                                 publicState.ObjectsTotal);
        }

        protected override void ServerInitializeEvent(ServerInitializeData data)
        {
            var privateState = data.PrivateState;
            var publicState = data.PublicState;

            privateState.Init();

            if (data.IsFirstTimeInit
                || publicState.IsSpawnCompleted)
            {
                return;
            }

            // the event was not properly initialized (spawn was not finished)
            // destroy the already spawned objects and restart the spawn
            foreach (var spawnedObject in privateState.SpawnedWorldObjects)
            {
                if (!spawnedObject.IsDestroyed)
                {
                    Server.World.DestroyObject(spawnedObject);
                }
            }

            this.ServerSpawnObjectsAsync(data.GameObject,
                                         publicState.AreaCirclePosition,
                                         publicState.AreaCircleRadius);
        }

        protected override bool ServerIsValidEventPosition(Vector2Ushort tilePosition)
        {
            if (Server.World.IsObservedByAnyPlayer(tilePosition))
            {
                return false;
            }

            foreach (var protoObjectToSpawn in this.SpawnPreset)
            {
                if (!ServerCheckCanSpawn(protoObjectToSpawn, tilePosition))
                {
                    return false;
                }
            }

            return true;
        }

        protected abstract bool ServerIsValidSpawnPosition(Vector2Ushort spawnPosition);

        protected virtual void ServerOnDropEventStarted(ILogicObject worldEvent)
        {
        }

        protected override void ServerOnEventDestroyed(ILogicObject worldEvent)
        {
            // destroy all the spawned objects
            foreach (var spawnedObject in GetPrivateState(worldEvent).SpawnedWorldObjects)
            {
                if (!spawnedObject.IsDestroyed)
                {
                    Server.World.DestroyObject(spawnedObject);
                }
            }
        }

        protected sealed override void ServerOnEventWithAreaStarted(ILogicObject worldEvent)
        {
            var publicState = GetPublicState(worldEvent);

            this.ServerSpawnObjectsAsync(worldEvent,
                                         publicState.AreaCirclePosition,
                                         publicState.AreaCircleRadius);

            this.ServerOnDropEventStarted(worldEvent);
        }

        protected abstract void ServerPrepareDropEvent(Triggers triggers, List<IProtoWorldObject> spawnPreset);

        protected sealed override void ServerPrepareEvent(Triggers triggers)
        {
            var list = new List<IProtoWorldObject>();
            this.ServerPrepareDropEvent(triggers, list);
            Api.Assert(list.Count > 0, "Spawn preset cannot be empty");

            foreach (var protoWorldObject in list)
            {
                switch (protoWorldObject)
                {
                    case IProtoCharacterMob:
                    case IProtoStaticWorldObject:
                        // supported types
                        continue;

                    default:
                        throw new Exception("Unknown object type in the spawn list: " + protoWorldObject);
                }
            }

            this.SpawnPreset = list;
        }

        protected override void ServerTryFinishEvent(ILogicObject worldEvent)
        {
            var canFinish = true;

            foreach (var spawnedObject in GetPrivateState(worldEvent).SpawnedWorldObjects)
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
                base.ServerTryFinishEvent(worldEvent);
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            var publicState = data.PublicState;
            if (!publicState.IsSpawnCompleted)
            {
                return;
            }

            var worldEvent = data.GameObject;
            ServerRefreshEventState(worldEvent);

            if (publicState.ObjectsRemains == 0)
            {
                this.ServerTryFinishEvent(worldEvent);
            }
        }

        private static bool ServerCheckCanSpawn(IProtoWorldObject protoObjectToSpawn, Vector2Ushort spawnPosition)
        {
            return protoObjectToSpawn switch
            {
                IProtoCharacterMob
                    => ServerCharacterSpawnHelper.IsPositionValidForCharacterSpawn(
                           spawnPosition.ToVector2D(),
                           isPlayer: false)
                       && !LandClaimSystem.SharedIsLandClaimedByAnyone(spawnPosition),

                IProtoStaticWorldObject protoStaticWorldObject
                    // Please note: land claim check must be integrated in the object tile requirements
                    => protoStaticWorldObject.CheckTileRequirements(
                        spawnPosition,
                        character: null,
                        logErrors: false),

                _ => throw new ArgumentOutOfRangeException("Unknown object type to spawn: " + protoObjectToSpawn)
            };
        }

        private static void ServerRefreshEventState(ILogicObject worldEvent)
        {
            var privateState = GetPrivateState(worldEvent);
            var publicState = GetPublicState(worldEvent);

            var countDestroyed = 0;
            var totalCount = privateState.SpawnedWorldObjects.Count;

            foreach (var spawnedObject in privateState.SpawnedWorldObjects)
            {
                if (spawnedObject.IsDestroyed)
                {
                    countDestroyed++;
                }
            }

            publicState.ObjectsRemains = (byte)Math.Min(byte.MaxValue, totalCount - countDestroyed);
            publicState.ObjectsTotal = (byte)Math.Min(byte.MaxValue,   totalCount);
        }

        private static IWorldObject ServerTrySpawn(IProtoWorldObject protoObjectToSpawn, Vector2Ushort spawnPosition)
        {
            return protoObjectToSpawn switch
            {
                IProtoCharacterMob protoCharacterMob
                    => Server.Characters.SpawnCharacter(protoCharacterMob,
                                                        spawnPosition.ToVector2D()),

                IProtoStaticWorldObject protoStaticWorldObject
                    => Server.World.CreateStaticWorldObject(
                        protoStaticWorldObject,
                        spawnPosition),

                _ => throw new Exception("Unknown object type to spawn: " + protoObjectToSpawn)
            };
        }

        private async void ServerSpawnObjectsAsync(
            ILogicObject worldEvent,
            Vector2Ushort circlePosition,
            ushort circleRadius)
        {
            var publicState = GetPublicState(worldEvent);
            Api.Assert(!publicState.IsSpawnCompleted, "Spawn already completed");

            var privateState = GetPrivateState(worldEvent);
            var spawnedObjects = privateState.SpawnedWorldObjects;
            var sqrMinDistanceBetweenSpawnedObjects = this.MinDistanceBetweenSpawnedObjects
                                                      * this.MinDistanceBetweenSpawnedObjects;

            var spawnedCount = 0;
            var yieldIfOutOfTime = (Func<Task>)Api.Server.Core.YieldIfOutOfTime;

            var protoObjectToSpawns = this.SpawnPreset;
            Logger.Important(
                $"Started async objects spawn for world event {worldEvent}: {this.SpawnPreset.Count} objects to spawn");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                foreach (var protoObjectToSpawn in protoObjectToSpawns)
                {
                    var attemptsRemains = 5000;
                    do
                    {
                        await yieldIfOutOfTime();

                        var spawnPosition = SharedCircleLocationHelper.SharedSelectRandomPositionInsideTheCircle(
                            circlePosition,
                            circleRadius);
                        if (!this.ServerIsValidSpawnPosition(spawnPosition))
                        {
                            // doesn't match any specific checks determined by the inheritor (such as a zone test)
                            continue;
                        }

                        var isTooClose = false;
                        foreach (var obj in spawnedObjects)
                        {
                            if (spawnPosition.TileSqrDistanceTo(obj.TilePosition)
                                > sqrMinDistanceBetweenSpawnedObjects)
                            {
                                continue;
                            }

                            isTooClose = true;
                            break;
                        }

                        if (isTooClose)
                        {
                            continue;
                        }

                        if (!ServerCheckCanSpawn(protoObjectToSpawn, spawnPosition))
                        {
                            // doesn't match the tile requirements or inside a claimed land area
                            continue;
                        }

                        if (Server.World.IsObservedByAnyPlayer(spawnPosition))
                        {
                            // observed by players
                            continue;
                        }

                        var spawnedObject = ServerTrySpawn(protoObjectToSpawn, spawnPosition);
                        spawnedObjects.Add(spawnedObject);
                        spawnedCount++;
                        Logger.Important($"Spawned world object: {spawnedObject} for world event {worldEvent}");
                        break;
                    }
                    while (--attemptsRemains > 0);

                    if (attemptsRemains == 0)
                    {
                        Logger.Error(
                            $"Cannot spawn world object: {protoObjectToSpawn} for world event {worldEvent}");
                    }
                }
            }
            finally
            {
                publicState.IsSpawnCompleted = true;
                Logger.Important(
                    $"Completed async objects spawn for world event {worldEvent}: {spawnedCount}/{protoObjectToSpawns.Count} objects spawned. Total time: {stopwatch.ElapsedMilliseconds} ms");

                ServerRefreshEventState(worldEvent);
            }
        }
    }
}