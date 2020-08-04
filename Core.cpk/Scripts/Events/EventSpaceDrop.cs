namespace AtomicTorch.CBND.CoreMod.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Events.Base;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events;
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

    public class EventSpaceDrop : ProtoEventDrop
    {
        private static Lazy<IReadOnlyList<IServerZone>> serverSpawnZones;

        public override ushort AreaRadius => PveSystem.ServerIsPvE
                                                 ? (ushort)64
                                                 : (ushort)90;

        public override string Description =>
            @"Space debris has fallen in the highlighted map area.
              [br]Rush in to collect some valuable junk!";

        public override TimeSpan EventDuration => TimeSpan.FromMinutes(30);

        public override double MinDistanceBetweenSpawnedObjects => 32;

        [NotLocalizable]
        public override string Name => "Space debris";

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
                && Server.Game.HoursSinceWorldCreation < 2)
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
                publicState.AreaCircleRadius * 1.2,
                duration: TimeSpan.FromHours(12));
        }

        protected override void ServerOnEventStartRequested(BaseTriggerConfig triggerConfig)
        {
            int locationsCount;
            if (PveSystem.ServerIsPvE)
            {
                locationsCount = 9;
            }
            else
            {
                locationsCount = Api.Server.Characters.OnlinePlayersCount >= 100 ? 4 : 3;
            }

            for (var index = 0; index < locationsCount; index++)
            {
                if (!this.ServerCreateAndStartEventInstance())
                {
                    break;
                }
            }
        }

        protected override Vector2Ushort ServerPickEventPosition(ILogicObject activeEvent)
        {
            var world = Server.World;
            using var tempExistingEventsSameType = Api.Shared.WrapInTempList(
                world.GetGameObjectsOfProto<ILogicObject, IProtoEvent>(
                    this));

            using var tempAllActiveEvents = Api.Shared.WrapInTempList(
                world.GetGameObjectsOfProto<ILogicObject, IProtoEventWithArea>());

            for (var globalAttempt = 0; globalAttempt < 5; globalAttempt++)
            {
                // try to select a zone which doesn't contain an active event of the same type
                var attempts = 25;
                IServerZone zoneInstance;
                do
                {
                    zoneInstance = this.ServerSelectRandomZoneWithEvenDistribution(serverSpawnZones.Value);
                    if (ServerCheckNoEventsInZone(zoneInstance, tempExistingEventsSameType.AsList()))
                    {
                        break;
                    }

                    zoneInstance = null;
                }
                while (--attempts > 0);

                if (zoneInstance is null)
                {
                    zoneInstance = this.ServerSelectRandomZoneWithEvenDistribution(serverSpawnZones.Value)
                                   ?? throw new Exception("Unable to pick an event position");
                }

                // pick up a valid position inside the zone
                var maxAttempts = 350;
                attempts = maxAttempts;
                do
                {
                    var result = zoneInstance.GetRandomPosition(RandomHelper.Instance);
                    if (this.ServerIsValidEventPosition(result)
                        && !ServerEventLocationManager.IsLocationUsedRecently(
                            result, 
                            this.AreaRadius * 4 * (attempts / (double)maxAttempts))
                        && this.ServerCheckNoEventsNearby(
                            result,
                            this.AreaRadius * (1 + 3 * (attempts / (double)maxAttempts)),
                            tempAllActiveEvents.AsList()))
                    {
                        return result;
                    }
                }
                while (--attempts > 0);
            }

            throw new Exception("Unable to pick an event position");
        }

        protected override void ServerPrepareDropEvent(
            Triggers triggers,
            List<IProtoWorldObject> spawnPreset)
        {
            triggers
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>()
                         .Configure(
                                 this.ServerGetIntervalForThisEvent(defaultInterval:
                                                                    (from: TimeSpan.FromHours(3),
                                                                     to: TimeSpan.FromHours(4.5)))
                             ));

            var debrisToSpawn = PveSystem.ServerIsPvE ? 2 : 3;
            for (var index = 0; index < debrisToSpawn; index++)
            {
                spawnPreset.Add(Api.GetProtoEntity<ObjectSpaceDebris>());
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
            AddZone(Api.GetProtoEntity<ZoneTemperateBarren>());
            AddZone(Api.GetProtoEntity<ZoneTemperateSwamp>());

            void AddZone(IProtoZone zone)
            {
                var instance = zone.ServerZoneInstance;
                result.Add(instance);
            }

            return result;
        }
    }
}