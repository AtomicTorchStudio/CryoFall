namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Zones;

    public abstract class ProtoZoneScript<TScriptConfig> : ProtoEntity, IZoneScript
        where TScriptConfig : IZoneScriptConfig
    {
        public override string Name => "Zone script: " + this.ShortId;

        public IReadOnlyList<BaseTriggerConfig> Triggers { get; private set; }

        public abstract Task ServerInvoke(TScriptConfig config, IProtoTrigger trigger, IServerZone zone);

        protected sealed override void PrepareProto()
        {
            if (IsClient)
            {
                // zone scripts are prepared only on the Server-side
                return;
            }

            var triggers = new Triggers();
            this.PrepareProtoZone(triggers);
            this.Triggers = triggers.ToReadOnly();
        }

        protected abstract void PrepareProtoZone(Triggers triggers);
    }
}