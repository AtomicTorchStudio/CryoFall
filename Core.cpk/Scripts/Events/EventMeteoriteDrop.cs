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
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EventMeteoriteDrop : ProtoEventDrop
    {
        private static readonly Lazy<IReadOnlyList<IServerZone>> ServerSpawnZones
            = new Lazy<IReadOnlyList<IServerZone>>(ServerSetupSpawnZones);

        public override ushort AreaRadius => 100;

        public override string Description =>
            @"Meteorites have fallen in the highlighted map area.
              [br]Rush in to mine some rare minerals!";

        public override TimeSpan EventDuration => TimeSpan.FromMinutes(30);

        public override double MinDistanceBetweenSpawnedObjects => 25;

        [NotLocalizable]
        public override string Name => "Meteorite";

        public override bool ServerIsTriggerAllowed(ProtoTrigger trigger)
        {
            if (ServerSpawnZones.Value.All(z => z.IsEmpty))
            {
                Logger.Error("All zones are empty (not mapped in the world), no place to start the event: " + this);
                return false;
            }

            if (trigger is TriggerTimeInterval
                && Server.Game.HoursSinceWorldCreation < 1)
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

        protected override Vector2Ushort ServerPickEventPosition(ILogicObject activeEvent)
        {
            var zoneInstance = ServerSpawnZones.Value.TakeByRandom();

            var attempts = 1000;
            do
            {
                var result = zoneInstance.GetRandomPosition(RandomHelper.Instance);
                if (this.ServerIsValidEventPosition(result))
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

            // 5 meteorites
            for (var index = 0; index < 5; index++)
            {
                spawnPreset.Add(Api.GetProtoEntity<ObjectMeteorite>());
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