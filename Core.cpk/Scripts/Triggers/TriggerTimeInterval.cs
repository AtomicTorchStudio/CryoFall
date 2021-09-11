namespace AtomicTorch.CBND.CoreMod.Triggers
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class TriggerTimeInterval : ProtoTrigger
    {
        private static TriggerTimeInterval instance;

        [NotLocalizable]
        public override string Name => "Time interval trigger";

        public static void ApplyPostpone(BaseTriggerConfig triggerConfig, double duration)
        {
            var config = (TriggerTimeIntervalConfig)triggerConfig;
            config.SetNextTriggerTime(Server.Game.FrameTime + duration);
        }

        public static BaseTriggerConfig ServerConfigureAndRegister(
            TimeSpan interval,
            Action callback,
            string name)
        {
            var config = instance.Configure(interval);
            config.ServerRegister(callback, name);
            return config;
        }

        public BaseTriggerConfig Configure(
            TimeSpan interval,
            bool adjustRateToPlayersNumber = false)
        {
            var seconds = interval.TotalSeconds;
            return new TriggerTimeIntervalConfig(trigger: this,
                                                 intervalFromSeconds: seconds,
                                                 intervalToSeconds: seconds,
                                                 adjustRateToPlayersNumber: adjustRateToPlayersNumber);
        }

        public BaseTriggerConfig Configure(
            TimeSpan intervalFrom,
            TimeSpan intervalTo,
            bool adjustRateToPlayersNumber = false)
        {
            return new TriggerTimeIntervalConfig(trigger: this,
                                                 intervalFromSeconds: intervalFrom.TotalSeconds,
                                                 intervalToSeconds: intervalTo.TotalSeconds,
                                                 adjustRateToPlayersNumber: adjustRateToPlayersNumber);
        }

        public BaseTriggerConfig Configure(
            (TimeSpan from, TimeSpan to) interval,
            bool adjustRateToPlayersNumber = false)
        {
            return new TriggerTimeIntervalConfig(trigger: this,
                                                 intervalFromSeconds: interval.from.TotalSeconds,
                                                 intervalToSeconds: interval.to.TotalSeconds,
                                                 adjustRateToPlayersNumber: adjustRateToPlayersNumber);
        }

        public BaseTriggerConfig ConfigureForSpawn(TimeSpan intervalFrom, TimeSpan intervalTo)
        {
            return this.Configure(intervalFrom, intervalTo, adjustRateToPlayersNumber: true);
        }

        public BaseTriggerConfig ConfigureForSpawn(TimeSpan interval)
        {
            return this.Configure(interval, adjustRateToPlayersNumber: true);
        }

        public override void ServerUpdate()
        {
            this.ServerUpdateConfigurations();
        }

        protected override void PrepareProto()
        {
            base.PrepareProto();
            instance = this;
        }

        private class TriggerTimeIntervalConfig : BaseTriggerConfig
        {
            private readonly bool adjustRateToPlayersNumber;

            private readonly double intervalFromSeconds;

            private readonly double intervalToSeconds;

            private double nextTriggerTime;

            public TriggerTimeIntervalConfig(
                TriggerTimeInterval trigger,
                double intervalFromSeconds,
                double intervalToSeconds,
                bool adjustRateToPlayersNumber)
                : base(trigger)
            {
                if (intervalToSeconds < intervalFromSeconds)
                {
                    throw new Exception("Time interval 'to' is less than 'from'");
                }

                this.intervalFromSeconds = intervalFromSeconds;
                this.intervalToSeconds = intervalToSeconds;
                this.adjustRateToPlayersNumber = adjustRateToPlayersNumber;

                // Schedule random trigger time (but not later than the "interval to" duration).
                // This is necessary to prevent the CPU spikes and to ensure that the spawn scripts are not executed
                // at the predictable time after the server restart.
                // Otherwise all the triggers of the same interval are triggering at exactly the same time.
                // TODO: actually it might be not a great idea in case of a spawn scripts which might need to keep a proper order
                this.nextTriggerTime = Server.Game.FrameTime
                                       + (RandomHelper.NextDouble() * intervalToSeconds);
            }

            public override void ServerUpdateConfiguration()
            {
                if (this.nextTriggerTime > Server.Game.FrameTime)
                {
                    return;
                }

                var fromSeconds = this.intervalFromSeconds;
                var toSeconds = this.intervalToSeconds;

                if (this.adjustRateToPlayersNumber)
                {
                    fromSeconds = ServerSpawnRateScaleHelper.AdjustDurationByRate(fromSeconds);
                    toSeconds = ServerSpawnRateScaleHelper.AdjustDurationByRate(toSeconds);
                }

                this.nextTriggerTime = Server.Game.FrameTime + fromSeconds;

                // add random interval (if necessary)
                var deltaInterval = toSeconds - fromSeconds;
                if (deltaInterval >= float.Epsilon)
                {
                    this.nextTriggerTime += deltaInterval * RandomHelper.NextDouble();
                }

                this.ServerInvokeTrigger();
            }

            public void SetNextTriggerTime(double time)
            {
                this.nextTriggerTime = time;
            }
        }
    }
}