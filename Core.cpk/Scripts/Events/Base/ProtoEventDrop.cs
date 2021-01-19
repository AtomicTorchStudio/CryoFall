namespace AtomicTorch.CBND.CoreMod.Events
{
    using System;
    using System.Collections.Generic;
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

        public override string SharedGetProgressText(ILogicObject activeEvent)
        {
            var publicState = GetPublicState(activeEvent);
            return string.Format(ProgressTextFormat,
                                 publicState.ObjectsTotal - publicState.ObjectsRemains,
                                 publicState.ObjectsTotal);
        }

        protected override void ServerInitializeEvent(ServerInitializeData data)
        {
            data.PrivateState.Init();
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

        protected virtual void ServerOnDropEventStarted(ILogicObject activeEvent)
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

            this.ServerOnDropEventStarted(activeEvent);

            ServerRefreshEventState(activeEvent);
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

        protected virtual void ServerSpawnObjects(
            ILogicObject activeEvent,
            Vector2Ushort circlePosition,
            ushort circleRadius,
            List<IWorldObject> spawnedObjects)
        {
            var sqrMinDistanceBetweenSpawnedObjects =
                this.MinDistanceBetweenSpawnedObjects * this.MinDistanceBetweenSpawnedObjects;

            foreach (var protoObjectToSpawn in this.SpawnPreset)
            {
                TrySpawn();

                void TrySpawn()
                {
                    var attempts = 2_000;
                    do
                    {
                        var spawnPosition =
                            SharedCircleLocationHelper.SharedSelectRandomPositionInsideTheCircle(
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
            ServerRefreshEventState(activeEvent);

            if (data.PublicState.ObjectsRemains == 0)
            {
                this.ServerTryFinishEvent(activeEvent);
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

        private static void ServerRefreshEventState(ILogicObject activeEvent)
        {
            var privateState = GetPrivateState(activeEvent);
            var publicState = GetPublicState(activeEvent);

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
    }
}