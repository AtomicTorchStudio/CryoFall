namespace AtomicTorch.CBND.CoreMod.Triggers
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.GameEngine.Common.DataStructures;

    /// <summary>
    /// Please note - the triggers are available only on the Server-side.
    /// </summary>
    public abstract class ProtoTrigger : ProtoEntity, IProtoTrigger
    {
        private readonly FreezableListWrapper<BaseTriggerConfig> configurations =
            new(new List<BaseTriggerConfig>());

        public void ServerRegisterConfiguration(BaseTriggerConfig triggerConfig)
        {
            this.configurations.OriginalList.Add(triggerConfig);
        }

        public void ServerUnregisterConfiguration(BaseTriggerConfig triggerConfig)
        {
            this.configurations.OriginalList.Remove(triggerConfig);
        }

        public abstract void ServerUpdate();

        protected void ServerUpdateConfigurations()
        {
            foreach (var configuration in this.configurations.FrozenList)
            {
                configuration.ServerUpdateConfiguration();
            }
        }
    }
}