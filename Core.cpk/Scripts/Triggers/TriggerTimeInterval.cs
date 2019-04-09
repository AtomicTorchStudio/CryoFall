namespace AtomicTorch.CBND.CoreMod.Triggers
{
    using System;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class TriggerTimeInterval : ProtoTrigger
    {
        private static TriggerTimeInterval instance;

        [NotLocalizable]
        public override string Name => "Time interval trigger";

        public static BaseTriggerConfig ServerConfigureAndRegister(
            TimeSpan interval,
            Action callback,
            string name)
        {
            var config = instance.Configure(interval);
            config.ServerRegister(callback, name);
            return config;
        }

        public BaseTriggerConfig Configure(TimeSpan interval)
        {
            var seconds = interval.TotalSeconds;
            return new TriggerTimeIntervalConfig(trigger: this,
                                                 intervalFromSeconds: seconds,
                                                 intervalToSeconds: seconds);
        }

        public BaseTriggerConfig Configure(TimeSpan intervalFrom, TimeSpan intervalTo)
        {
            return new TriggerTimeIntervalConfig(trigger: this,
                                                 intervalFromSeconds: intervalFrom.TotalSeconds,
                                                 intervalToSeconds: intervalTo.TotalSeconds);
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
            private readonly double intervalFromSeconds;

            private readonly double intervalToSeconds;

            private double nextTriggerTime;

            public TriggerTimeIntervalConfig(
                TriggerTimeInterval trigger,
                double intervalFromSeconds,
                double intervalToSeconds)
                : base(trigger)
            {
                if (intervalToSeconds < intervalFromSeconds)
                {
                    throw new Exception("Time interval 'to' is less than 'from'");
                }

                this.intervalFromSeconds = intervalFromSeconds;
                this.intervalToSeconds = intervalToSeconds;

                // Trigger sometime soon (in the interval range).
                // This is necessary to prevent the CPU spikes.
                // Otherwise all the triggers of the same interval are triggering at exactly the same time.
                // TODO: actually it might be not a great idea in case of a spawn scripts which might need to keep a proper order
                this.nextTriggerTime = Server.Game.FrameTime
                                       + (RandomHelper.NextDouble() * intervalFromSeconds);
            }

            public override void ServerUpdateConfiguration()
            {
                if (this.nextTriggerTime > Server.Game.FrameTime)
                {
                    return;
                }

                this.nextTriggerTime = Server.Game.FrameTime + this.intervalFromSeconds;

                // add random interval (if necessary)
                var deltaInterval = this.intervalToSeconds - this.intervalFromSeconds;
                if (deltaInterval >= float.Epsilon)
                {
                    this.nextTriggerTime += deltaInterval * RandomHelper.NextDouble();
                }

                this.ServerInvokeTrigger();
            }
        }
    }
}