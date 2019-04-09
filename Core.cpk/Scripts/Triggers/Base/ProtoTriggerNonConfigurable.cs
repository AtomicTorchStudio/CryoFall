namespace AtomicTorch.CBND.CoreMod.Triggers
{
    public abstract class ProtoTriggerNonConfigurable : ProtoTrigger
    {
        protected ProtoTriggerNonConfigurable()
        {
            this.DefaultConfiguration = new EmptyTriggerConfig(this);
        }

        public BaseTriggerConfig DefaultConfiguration { get; }

        public override void ServerUpdate()
        {
            // not used
        }

        protected void Invoke()
        {
            // simply invoke the trigger during configurations update
            this.ServerUpdateConfigurations();
        }

        private class EmptyTriggerConfig : BaseTriggerConfig
        {
            public EmptyTriggerConfig(ProtoTrigger trigger) : base(trigger)
            {
            }

            public override void ServerUpdateConfiguration()
            {
                // simply invoke the trigger
                this.ServerInvokeTrigger();
            }
        }
    }
}