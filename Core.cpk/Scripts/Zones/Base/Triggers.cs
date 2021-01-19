namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class Triggers
    {
        private readonly List<BaseTriggerConfig> triggers = new();

        public Triggers Add(BaseTriggerConfig configuredTrigger)
        {
            this.triggers.Add(configuredTrigger);
            return this;
        }

        public Triggers Add(ProtoTriggerNonConfigurable nonConfigurableTrigger)
        {
            this.triggers.Add(nonConfigurableTrigger.DefaultConfiguration);
            return this;
        }

        public IReadOnlyList<BaseTriggerConfig> ToReadOnly()
        {
            this.triggers.TrimExcess();
            return this.triggers;
        }
    }
}