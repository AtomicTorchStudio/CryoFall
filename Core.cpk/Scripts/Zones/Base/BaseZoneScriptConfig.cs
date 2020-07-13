namespace AtomicTorch.CBND.CoreMod.Zones
{
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class BaseZoneScriptConfig<TProtoConfig> : IZoneScriptConfig
        where TProtoConfig : BaseZoneScriptConfig<TProtoConfig>
    {
        protected BaseZoneScriptConfig(ProtoZoneScript<TProtoConfig> zoneScript)
        {
            this.ZoneScript = zoneScript;
        }

        public string ShortId => this.ZoneScript.ShortId;

        public ProtoZoneScript<TProtoConfig> ZoneScript { get; }

        IZoneScript IZoneScriptConfig.ZoneScript => this.ZoneScript;

        public void ServerInvoke(IProtoTrigger trigger, IServerZone serverZoneInstance)
        {
            this.ZoneScript.ServerInvoke((TProtoConfig)this, trigger, serverZoneInstance);
        }

        public void ServerRegisterZone(IProtoZone zone)
        {
            foreach (var triggerConfig in this.ZoneScript.Triggers)
            {
                triggerConfig.ServerRegister(
                    callback: () => this.TriggerCallback(zone, triggerConfig),
                    $"ZoneScript.{this.GetType().Name}.{this.ShortId}");
            }
        }

        // ReSharper disable once CanExtractXamlLocalizableStringCSharp
        public override string ToString() => "Script config for " + this.ShortId;

        private void TriggerCallback(IProtoZone zone, BaseTriggerConfig triggerConfig)
        {
            if (Api.IsEditor)
            {
                //Api.Logger.Write(
                //	string.Format(
                //		"Trigger suppressed {0}: should invoke {1} for {2}{3}(suppressed because you\'re running the Editor executable)",
                //		triggerConfig.ShortId,
                //		this.ShortId,
                //		zone.ShortId,
                //		Environment.NewLine));
                return;
            }

            var serverZoneInstance = zone.ServerZoneInstance;
            //Api.Logger.Info($"Trigger invoke {triggerConfig.ShortId}: invoking {this.ShortId} for {zone.ShortId}");
            this.ServerInvoke(triggerConfig.Trigger, serverZoneInstance);
        }
    }
}