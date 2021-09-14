﻿namespace AtomicTorch.CBND.CoreMod.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EventMigrationFloater : ProtoEventDrop
    {
        private static Lazy<IReadOnlyList<(IServerZone Zone, uint Weight)>> serverSpawnZones;

        public override ushort AreaRadius => PveSystem.ServerIsPvE
                                             && !Server.Core.IsLocalServer
                                                 ? (ushort)64
                                                 : (ushort)90;

        public override string Description =>
            "Native lifeforms of this world seem to be migrating to a different area, giving a great opportunity for hunting.";

        public override TimeSpan EventDuration => TimeSpan.FromMinutes(30);

        public override double MinDistanceBetweenSpawnedObjects => 11;

        public override string Name => "Floater migration";

        protected override double DelayHoursSinceWipe => 24 * RateWorldEventInitialDelayMultiplier.SharedValue;

        public override bool ServerIsTriggerAllowed(ProtoTrigger trigger)
        {
            if (trigger is not null
                && (this.ServerHasAnyEventOfType<IProtoEvent>()
                    || ServerHasAnyEventOfTypeRunRecently<IProtoEvent>(TimeSpan.FromMinutes(20))))
            {
                // this event cannot run together or start soon after any other event
                return false;
            }

            if (serverSpawnZones.Value.All(z => z.Zone.IsEmpty))
            {
                Logger.Error("All zones are empty (not mapped in the world), no place to start the event: " + this);
                return false;
            }

            return true;
        }

        protected override bool ServerIsValidSpawnPosition(Vector2Ushort spawnPosition)
        {
            foreach (var serverZone in serverSpawnZones.Value)
            {
                if (serverZone.Zone.IsContainsPosition(spawnPosition))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void ServerOnDropEventStarted(ILogicObject worldEvent)
        {
            var publicState = GetPublicState(worldEvent);
            ServerEventLocationManager.AddUsedLocation(
                publicState.AreaCirclePosition,
                publicState.AreaCircleRadius * 1.2,
                duration: TimeSpan.FromHours(8));
        }

        protected override void ServerOnEventStartRequested(BaseTriggerConfig triggerConfig)
        {
            int locationsCount;
            if (PveSystem.ServerIsPvE
                && !Server.Core.IsLocalServer)
            {
                locationsCount = 9;
            }
            else
            {
                locationsCount = Api.Server.Characters.OnlinePlayersCount >= 100 ? 3 : 2;
            }

            for (var index = 0; index < locationsCount; index++)
            {
                if (!this.ServerCreateAndStartEventInstance())
                {
                    break;
                }
            }
        }

        protected override Vector2Ushort ServerPickEventPosition(ILogicObject worldEvent)
        {
            var world = Server.World;
            using var tempExistingEventsSameType = Api.Shared.WrapInTempList(
                world.GetGameObjectsOfProto<ILogicObject, IProtoEvent>(
                    this));

            using var tempAllWorldEvents = Api.Shared.WrapInTempList(
                world.GetGameObjectsOfProto<ILogicObject, IProtoEventWithArea>());

            for (var globalAttempt = 0; globalAttempt < 10; globalAttempt++)
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

                zoneInstance ??= this.ServerSelectRandomZoneWithEvenDistribution(serverSpawnZones.Value)
                                 ?? throw new Exception("Unable to pick an event position");

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
                            tempAllWorldEvents.AsList()))
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
            triggers.Add(GetTrigger<TriggerTimeInterval>()
                             .Configure(RateWorldEventIntervalMigrationFloater.SharedValueIntervalHours));

            var mobsToSpawn = PveSystem.ServerIsPvE
                              && !Server.Core.IsLocalServer
                                  ? 6
                                  : 12;
            for (var index = 0; index < mobsToSpawn; index++)
            {
                spawnPreset.Add(Api.GetProtoEntity<MobFloater>());
            }
        }

        protected override void ServerWorldChangedHandler()
        {
            serverSpawnZones = new Lazy<IReadOnlyList<(IServerZone, uint)>>(ServerSetupSpawnZones);
        }

        private static IReadOnlyList<(IServerZone, uint)> ServerSetupSpawnZones()
        {
            var result = new List<(IServerZone, uint)>();
            AddZone(Api.GetProtoEntity<ZoneTropicalForest>());
            AddZone(Api.GetProtoEntity<ZoneTemperateForest>());
            AddZone(Api.GetProtoEntity<ZoneBorealForest>());
            AddZone(Api.GetProtoEntity<ZoneTemperateBarren>());
            AddZone(Api.GetProtoEntity<ZoneTemperateSwamp>());

            void AddZone(IProtoZone zone)
            {
                var instance = zone.ServerZoneInstance;
                result.Add((instance, (uint)instance.PositionsCount));
            }

            return result;
        }
    }
}