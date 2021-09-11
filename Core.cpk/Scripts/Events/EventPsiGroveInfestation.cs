namespace AtomicTorch.CBND.CoreMod.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class EventPsiGroveInfestation
        : ProtoEvent<
            EventPsiGroveInfestation.PrivateState,
            EventPublicState,
            EmptyClientState>
    {
        public const string Description_FirearmsUseless
            = "Important: Firearms are useless; use melee or heat-based weapons.";

        public const string Description_Primary
            = "A strange alien growth has appeared in the mountainous areas making prospecting near impossible, but it is a great opportunity to obtain rare resources.";

        private static Lazy<IReadOnlyList<IServerZone>> serverSpawnZones;

        public override bool ConsolidateNotifications => false;

        public override string Description
            => Description_Primary
               + "[br][br]"
               + Description_FirearmsUseless;

        public override TimeSpan EventDuration => TimeSpan.FromMinutes(20);

        public override string Name => "Psi grove infestation";

        public SpawnConfig SpawnScriptConfig { get; private set; }

        protected override double DelayHoursSinceWipe => 12 * RateWorldEventInitialDelayMultiplier.SharedValue;

        public override bool ServerIsTriggerAllowed(ProtoTrigger trigger)
        {
            if (trigger is not null
                && (this.ServerHasAnyEventOfType<IProtoEvent>()
                    || ServerHasAnyEventOfTypeRunRecently<IProtoEvent>(TimeSpan.FromMinutes(20))))
            {
                // this event cannot run together or start soon after any other event
                return false;
            }

            if (this.ServerHasAnyEventOfType<EventPsiGroveInfestation>())
            {
                return false;
            }

            if (serverSpawnZones.Value.All(z => z.IsEmpty))
            {
                Logger.Error("All zones are empty (not mapped in the world), no place to start the event: " + this);
                return false;
            }

            return true;
        }

        protected override void ServerInitializeEvent(ServerInitializeData data)
        {
            data.PrivateState.Init();
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

        protected override void ServerOnEventStarted(ILogicObject worldEvent)
        {
            this.ServerSpawnObjects(worldEvent,
                                    GetPrivateState(worldEvent).SpawnedWorldObjects);
        }

        protected override void ServerPrepareEvent(Triggers triggers)
        {
            triggers.Add(GetTrigger<TriggerTimeInterval>()
                             .Configure(RateWorldEventIntervalPsiGroveInfestation.SharedValueIntervalHours));

            this.SpawnScriptConfig = Api.GetProtoEntity<SpawnEventPsiGroveInfestation>()
                                        .Configure(densityMultiplier: 1.0);
        }

        protected virtual async void ServerSpawnObjects(
            ILogicObject worldEvent,
            List<IWorldObject> spawnedObjects)
        {
            Logger.Important("Starting mobs spawn for " + worldEvent);

            var spawnScriptConfig = this.SpawnScriptConfig;
            var spawnScript = (ProtoZoneSpawnScript)spawnScriptConfig.ZoneScript;
            foreach (var zone in serverSpawnZones.Value)
            {
                await spawnScript.ServerInvoke(spawnScriptConfig, trigger: null, zone);
                var mobsTracker = SpawnedMobsTrackingManagersStore.Get(spawnScript, zone);
                spawnedObjects.AddRange(mobsTracker.EnumerateAll());
            }

            Logger.Important($"Finished mobs spawn for {worldEvent}: {spawnedObjects.Count} mobs spawned");
        }

        protected override void ServerTryFinishEvent(ILogicObject worldEvent)
        {
            var canFinish = true;

            var spawnedWorldObjects = GetPrivateState(worldEvent).SpawnedWorldObjects;
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
                base.ServerTryFinishEvent(worldEvent);
            }
        }

        protected override void ServerWorldChangedHandler()
        {
            serverSpawnZones = new Lazy<IReadOnlyList<IServerZone>>(ServerSetupSpawnZones);
        }

        private static List<IServerZone> ServerSetupSpawnZones()
        {
            return new()
            {
                // during the event, the psi grove appears in all mountainous areas
                Api.GetProtoEntity<ZoneBorealMountain>().ServerZoneInstance,
                Api.GetProtoEntity<ZoneTemperateMountain>().ServerZoneInstance,
                Api.GetProtoEntity<ZoneTropicalMountain>().ServerZoneInstance
            };
        }

        public class PrivateState : BasePrivateState
        {
            public List<IWorldObject> SpawnedWorldObjects { get; }
                = new();

            public void Init()
            {
                for (var index = 0; index < this.SpawnedWorldObjects.Count; index++)
                {
                    var worldObject = this.SpawnedWorldObjects[index];
                    if (worldObject is null)
                    {
                        this.SpawnedWorldObjects.RemoveAt(index--);
                    }
                }
            }
        }
    }
}