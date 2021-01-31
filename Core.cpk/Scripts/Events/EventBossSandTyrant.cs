namespace AtomicTorch.CBND.CoreMod.Events
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class EventBossSandTyrant : ProtoEventBoss
    {
        static EventBossSandTyrant()
        {
            ServerEventDelayHours = ServerRates.Get(
                "EventDelay.BossSandTyrant",
                defaultValue: 48.0,
                @"This hours value determines when the Sand Tyrant boss will start spawning for the first time.                  
                  Please note: for PvP server this value will be substituted by time-gating setting
                  for T4 specialized tech if it's larger than this value (as there is no viable way
                  for players to defeat the boss until T4 weapons becomes available).");
        }

        public override string Description =>
            @"Sand Tyrant has appeared on the surface.
              [br]Valuable loot awaits the bravest survivors!";

        public override TimeSpan EventDuration { get; }
            = TimeSpan.FromHours(1);

        [NotLocalizable]
        public override string Name => "Sand Tyrant";

        private static double ServerEventDelayHours { get; }

        public override bool ServerIsTriggerAllowedForBossEvent(ProtoTrigger trigger)
        {
            if (trigger is not null)
            {
                if (this.ServerHasAnyEventOfType<ProtoEventBoss>()
                    || ServerHasAnyEventOfTypeRunRecently<ProtoEventBoss>(TimeSpan.FromHours(3)))
                {
                    // another boss event is running now or run recently 
                    return false;
                }
            }

            if (trigger is TriggerTimeInterval)
            {
                var delayHoursSinceWipe = ServerEventDelayHours;
                if (!PveSystem.ServerIsPvE)
                {
                    // in PvP spawn Pragmium Queen not earlier than
                    // T4 specialized tech (containing the necessary weapons) becomes available
                    delayHoursSinceWipe = Math.Max(
                        delayHoursSinceWipe,
                        // convert seconds to hours
                        TechConstants.PvpTechTimeGameTier4Specialized / 3600);
                }

                if (Server.Game.HoursSinceWorldCreation < delayHoursSinceWipe)
                {
                    // too early
                    return false;
                }
            }

            return true;
        }

        protected override void ServerPrepareBossEvent(
            Triggers triggers,
            List<IProtoSpawnableObject> spawnPreset)
        {
            triggers
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>()
                         .Configure(
                                 this.ServerGetIntervalForThisEvent(defaultInterval:
                                                                    (from: TimeSpan.FromHours(5),
                                                                     to: TimeSpan.FromHours(10)))
                             ));

            spawnPreset.Add(
                Api.GetProtoEntity<MobBossSandTyrant>());
        }
    }
}