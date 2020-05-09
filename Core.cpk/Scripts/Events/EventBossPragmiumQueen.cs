namespace AtomicTorch.CBND.CoreMod.Events
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.Events.Base;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EventBossPragmiumQueen : ProtoEventBoss
    {
        private static readonly Lazy<IReadOnlyList<IServerZone>> ServerSpawnZones
            = new Lazy<IReadOnlyList<IServerZone>>(ServerSetupSpawnZones);

        static EventBossPragmiumQueen()
        {
            ServerEventDelayHours = ServerRates.Get(
                "BossEventDelayHoursPragmiumQueen",
                defaultValue: 48.0,
                @"This hours value determines when the Pragmium Queen boss will start spawning for the first time.                  
                  Please note: for PvP server this value will be substituted by time-gating setting
                  for T3 specialized tech if it's larger than this value (as there is no viable way
                  for players to defeat the boss until T3 weapons becomes available).");
        }

        public override ushort AreaRadius => 37;

        public override string Description =>
            @"Pragmium Queen has appeared on the surface.
              [br]Valuable loot awaits the bravest survivors!";

        public override TimeSpan EventDuration { get; }
            = TimeSpan.FromHours(1);

        [NotLocalizable]
        public override string Name => "Pragmium Queen";

        private static double ServerEventDelayHours { get; }

        public override bool ServerIsTriggerAllowed(ProtoTrigger trigger)
        {
            if (trigger != null
                && this.ServerHasAnyEventOfType<ProtoEventBoss>())
            {
                return false;
            }

            if (ServerSpawnZones.Value.All(z => z.IsEmpty))
            {
                Logger.Error("All zones are empty (not mapped in the world), no place to start the event: " + this);
                return false;
            }

            if (trigger is TriggerTimeInterval)
            {
                var delayHoursSinceWipe = ServerEventDelayHours;
                if (!PveSystem.ServerIsPvE)
                {
                    // in PvP spawn Pragmium Queen not earlier than
                    // T3 specialized tech (containing the necessary weapons) becomes available
                    delayHoursSinceWipe = Math.Max(
                        delayHoursSinceWipe,
                        // convert seconds to hours
                        TechConstants.PvpTechTimeGameTier3Specialized / 3600);
                }

                if (Server.Game.HoursSinceWorldCreation < delayHoursSinceWipe)
                {
                    // too early
                    return false;
                }
            }

            if (this.ServerIsSameEventExist())
            {
                Logger.Error("The same event is already running, cannot start a new one: " + this);
                return false;
            }

            return true;
        }

        protected override Vector2Ushort ServerPickEventPosition(ILogicObject activeEvent)
        {
            // use center position of the zone
            var zoneInstance = ServerSpawnZones.Value.TakeByRandom();
            return new Vector2Ushort((ushort)zoneInstance.AllPositions.Average(p => p.X),
                                     (ushort)zoneInstance.AllPositions.Average(p => p.Y));
        }

        protected override void ServerPrepareBossEvent(
            Triggers triggers,
            List<IProtoSpawnableObject> spawnPreset)
        {
            triggers
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>()
                         .Configure(
                             intervalFrom: TimeSpan.FromHours(7),
                             intervalTo: TimeSpan.FromHours(9)));

            spawnPreset.Add(Api.GetProtoEntity<MobPragmiumQueen>());
        }

        protected override Vector2Ushort ServerSelectSpawnPosition(Vector2Ushort circlePosition, ushort circleRadius)
        {
            return circlePosition;
        }

        private static IReadOnlyList<IServerZone> ServerSetupSpawnZones()
        {
            var result = new List<IServerZone>();
            AddZone(Api.GetProtoEntity<ZoneEventBossPragmiumQueen>());

            void AddZone(IProtoZone zone)
            {
                var instance = zone.ServerZoneInstance;
                result.Add(instance);
            }

            return result;
        }
    }
}