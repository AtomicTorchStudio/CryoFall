namespace AtomicTorch.CBND.CoreMod.Events
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.Zones;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class EventBossPragmiumQueen : ProtoEventBoss
    {
        public override string Description =>
            @"Pragmium Queen has appeared on the surface.
              [br]Valuable loot awaits the bravest survivors!";

        public override TimeSpan EventDurationWithoutDelay { get; }
            = TimeSpan.FromMinutes(30);

        public override TimeSpan EventStartDelayDuration
            => !SharedLocalServerHelper.IsLocalServer
                   ? TimeSpan.FromMinutes(20)
                   : TimeSpan.Zero;

        public override string Name => "Pragmium Queen";

        protected override double DelayHoursSinceWipe
        {
            get
            {
                var delayHours = 48.0; // 48 hours by default
                delayHours *= RateWorldEventInitialDelayMultiplier.SharedValue;

                if (PveSystem.ServerIsPvE)
                {
                    return delayHours;
                }

                // in PvP spawn boss not earlier than
                // T4 specialized tech (containing the necessary weapons) becomes available
                delayHours = Math.Max(
                    delayHours,
                    TechConstants.PvPTimeGates
                                 .Get(TechTier.Tier4, isSpecialized: true)
                    / 3600);

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
            triggers.Add(GetTrigger<TriggerTimeInterval>()
                             .Configure(RateWorldEventIntervalBossPragmiumQueen.SharedValueIntervalHours));

            spawnPreset.Add(Api.GetProtoEntity<MobBossPragmiumQueen>());
        }
    }
}