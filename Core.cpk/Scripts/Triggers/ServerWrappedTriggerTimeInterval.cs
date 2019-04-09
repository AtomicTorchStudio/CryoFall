namespace AtomicTorch.CBND.CoreMod.Triggers
{
    using System;

    public class ServerWrappedTriggerTimeInterval : IDisposable
    {
        private readonly Action callbackTimerTick;

        private BaseTriggerConfig triggerConfig;

        public ServerWrappedTriggerTimeInterval(
            Action callbackTimerTick,
            TimeSpan interval,
            string name)
        {
            this.callbackTimerTick = callbackTimerTick;
            this.triggerConfig = TriggerTimeInterval.ServerConfigureAndRegister(
                interval,
                this.ServerTimerTick,
                name);
        }

        public void Dispose()
        {
            this.triggerConfig.ServerUnregister();
            this.triggerConfig = null;
        }

        private void ServerTimerTick()
        {
            this.callbackTimerTick();
        }
    }
}