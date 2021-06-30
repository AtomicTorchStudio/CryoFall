namespace AtomicTorch.CBND.CoreMod.Events
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class EventBossPragmiumQueen : ProtoEventBoss
    {
        public override string Description =>
            @"Pragmium Queen has appeared on the surface.
              [br]Valuable loot awaits the bravest survivors!";

        public override TimeSpan EventDurationWithoutDelay { get; }
            = TimeSpan.FromHours(1);

        public override TimeSpan EventStartDelayDuration
            => PveSystem.SharedIsPve(true)
               && !SharedLocalServerHelper.IsLocalServer
                   ? TimeSpan.FromMinutes(20)
                   : TimeSpan.Zero;

        [NotLocalizable]
        public override string Name => "Pragmium Queen";

        protected override double DelayHoursSinceWipe
        {
            get
            {
                var delayHours = 48.0; // 48 hours by default
                delayHours *= EventConstants.ServerEventDelayMultiplier;

                if (PveSystem.ServerIsPvE)
                {
                    return delayHours;
                }

                // in PvP spawn boss not earlier than
                // T4 specialized tech (containing the necessary weapons) becomes available
                delayHours = Math.Max(
                    delayHours,
                    // convert seconds to hours
                    TechConstants.PvpTechTimeGameTier4Specialized / 3600);

                return delayHours;
            }
        }

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

            return true;
        }

        protected override void ServerPrepareBossEvent(
            Triggers triggers,
            List<IProtoSpawnableObject> spawnPreset)
        {
            // default interval is configured here (but can be adjusted in the ServerRates.config)
            double fromHours, toHours;
            if (SharedLocalServerHelper.IsLocalServer)
            {
                fromHours = 3;
                toHours = 5;
            }
            else
            {
                fromHours = 5;
                toHours = 10;
            }

            triggers
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>()
                         .Configure(
                                 this.ServerGetIntervalForThisEvent(defaultInterval:
                                                                    (from: TimeSpan.FromHours(fromHours),
                                                                     to: TimeSpan.FromHours(toHours)))
                             ));

            spawnPreset.Add(Api.GetProtoEntity<MobBossPragmiumQueen>());
        }
    }
}