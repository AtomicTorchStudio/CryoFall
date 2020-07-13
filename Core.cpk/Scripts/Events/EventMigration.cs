namespace AtomicTorch.CBND.CoreMod.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.Events.Base;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EventMigration : ProtoEventDrop
    {
        private static Lazy<IReadOnlyList<IServerZone>> serverSpawnZones;

        public override ushort AreaRadius => 90;

        public override string Description =>
            "Native lifeforms of this world seem to be migrating to a different area, giving a great opportunity for hunting.";

        public override TimeSpan EventDuration => TimeSpan.FromMinutes(30);

        public override double MinDistanceBetweenSpawnedObjects => 11;

        [NotLocalizable]
        public override string Name => "Migration";

        public override bool ServerIsTriggerAllowed(ProtoTrigger trigger)
        {
            if (trigger != null
                && this.ServerHasAnyEventOfType<ProtoEventDrop>())
            {
                return false;
            }

            if (serverSpawnZones.Value.All(z => z.IsEmpty))
            {
                Logger.Error("All zones are empty (not mapped in the world), no place to start the event: " + this);
                return false;
            }

            if (trigger is TriggerTimeInterval
                && Server.Game.HoursSinceWorldCreation < 24)
            {
                // too early
                return false;
            }

            return true;
        }

        protected override bool ServerIsValidSpawnPosition(Vector2Ushort spawnPosition)
        {
            foreach (var serverZone in serverSpawnZones.Value)
            {
                if (serverZone.IsContainsPosition(spawnPosition))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void ServerOnDropEventStarted(ILogicObject activeEvent)
        {
            var publicState = GetPublicState(activeEvent);
            ServerEventLocationManager.AddUsedLocation(
                publicState.AreaCirclePosition,
                publicState.AreaCircleRadius + 20,
                duration: TimeSpan.FromHours(12));
        }

        protected override void ServerOnEventStartRequested(BaseTriggerConfig triggerConfig)
        {
            var eventsCount = PveSystem.ServerIsPvE
                              || Api.Server.Characters.OnlinePlayersCount >= 100
                                  ? 4
                                  : 3;

            for (var i = 0; i < eventsCount; i++)
            {
                if (!this.ServerCreateAndStartEventInstance())
                {
                    break;
                }
            }
        }

        protected override Vector2Ushort ServerPickEventPosition(ILogicObject activeEvent)
        {
            for (var globalAttempt = 0; globalAttempt < 5; globalAttempt++)
            {
                // pick up a zone which doesn't contain an active event of the same type
                var attempts = 50;
                IServerZone zoneInstance;
                do
                {
                    zoneInstance = this.ServerSelectRandomZoneWithEvenDistribution(serverSpawnZones.Value);
                    if (this.ServerCheckNoSameEventsInZone(zoneInstance))
                    {
                        break;
                    }

                    zoneInstance = null;
                }
                while (--attempts > 0);

                if (zoneInstance is null)
                {
                    throw new Exception("Unable to pick an event position");
                }

                // pick up a valid position inside the zone
                attempts = 250;
                do
                {
                    var result = zoneInstance.GetRandomPosition(RandomHelper.Instance);
                    if (this.ServerIsValidEventPosition(result)
                        && !ServerEventLocationManager.IsLocationUsedRecently(result, this.AreaRadius)
                        && this.ServerCheckNoEventsNearby(result, this.AreaRadius * 4))
                    {
                        return result;
                    }
                }
                while (--attempts > 0);
            }

            throw new Exception("Unable to pick an event position");
        }

        protected override void ServerPrepareDropEvent(Triggers triggers, List<IProtoWorldObject> spawnPreset)
        {
            triggers
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>()
                         .Configure(
                                 this.ServerGetIntervalForThisEvent(defaultInterval:
                                                                    (from: TimeSpan.FromHours(4),
                                                                     to: TimeSpan.FromHours(8)))
                             ));

            for (var index = 0; index < 12; index++)
            {
                spawnPreset.Add(Api.GetProtoEntity<MobFloater>());
            }
        }

        protected override void ServerWorldChangedHandler()
        {
            serverSpawnZones = new Lazy<IReadOnlyList<IServerZone>>(ServerSetupSpawnZones);
        }

        private static IReadOnlyList<IServerZone> ServerSetupSpawnZones()
        {
            var result = new List<IServerZone>();
            AddZone(Api.GetProtoEntity<ZoneTropicalForest>());
            AddZone(Api.GetProtoEntity<ZoneTemperateForest>());
            AddZone(Api.GetProtoEntity<ZoneBorealForest>());
            AddZone(Api.GetProtoEntity<ZoneTemperateSwamp>());
            AddZone(Api.GetProtoEntity<ZoneTemperateBarren>());

            void AddZone(IProtoZone zone)
            {
                var instance = zone.ServerZoneInstance;
                result.Add(instance);
            }

            return result;
        }
    }
}