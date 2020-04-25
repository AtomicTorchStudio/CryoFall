namespace AtomicTorch.CBND.CoreMod.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Events.Base;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events;
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
        private static readonly Lazy<IReadOnlyList<IServerZone>> ServerSpawnZones
            = new Lazy<IReadOnlyList<IServerZone>>(ServerSetupSpawnZones);

        public override ushort AreaRadius => 90;

        public override string Description =>
            @"Space debris has fallen in the highlighted map area.
              [br]Rush in to collect some valuable junk!";

        public override TimeSpan EventDuration => TimeSpan.FromMinutes(30);

        public override double MinDistanceBetweenSpawnedObjects => 32;

        [NotLocalizable]
        public override string Name => "Space debris";

        public override bool ServerIsTriggerAllowed(ProtoTrigger trigger)
        {
            if (ServerSpawnZones.Value.All(z => z.IsEmpty))
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
            foreach (var serverZone in ServerSpawnZones.Value)
            {
                if (serverZone.IsContainsPosition(spawnPosition))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void ServerOnEventStartRequested(BaseTriggerConfig triggerConfig)
        {
            // create up to 2 simultaneous event instances if there are many online players
            var eventsCount = Api.Server.Characters.OnlinePlayersCount >= 100
                                  ? 2
                                  : 1;

            for (var i = 0; i < eventsCount; i++)
            {
                this.ServerCreateAndStartEventInstance();
            }
        }

        protected override Vector2Ushort ServerPickEventPosition(ILogicObject activeEvent)
        {
            // pick up a zone which doesn't contain an active event of the same type
            var attempts = 50;
            IServerZone zoneInstance;
            do
            {
                zoneInstance = this.ServerSelectRandomZoneWithEvenDistribution(ServerSpawnZones.Value);
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
            attempts = 1000;
            do
            {
                var result = zoneInstance.GetRandomPosition(RandomHelper.Instance);
                if (this.ServerIsValidEventPosition(result)
                    && this.ServerCheckNoEventsNearby(result, this.AreaRadius * 3.5))
                {
                    return result;
                }
            }
            while (--attempts > 0);

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
                             intervalFrom: TimeSpan.FromHours(3),
                             intervalTo: TimeSpan.FromHours(6)));

            // 3 debris
            for (var i = 0; i < 3; i++)
            {
                spawnPreset.Add(Api.GetProtoEntity<ObjectSpaceDebris>());
            }
        }

        private static IReadOnlyList<IServerZone> ServerSetupSpawnZones()
        {
            var result = new List<IServerZone>();
            AddZone(Api.GetProtoEntity<ZoneTemperateForest>());
            AddZone(Api.GetProtoEntity<ZoneBorealForest>());
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