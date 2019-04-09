namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;

    [PrepareOrder(afterType: typeof(IProtoTrigger))]
    [PrepareOrder(afterType: typeof(IZoneScript))]
    public abstract class ProtoZone<TPrivateState>
        : ProtoGameObject
          <IServerZone,
              TPrivateState,
              EmptyPublicState,
              EmptyClientState>,
          IProtoZone
        where TPrivateState : BasePrivateState, new()
    {
        public IReadOnlyList<IZoneScriptConfig> AttachedScripts { get; private set; }

        /// <summary>
        /// Client update is disabled.
        /// </summary>
        public sealed override double ClientUpdateIntervalSeconds => double.MaxValue;

        public override double ServerUpdateIntervalSeconds => 1;

        public IServerZone ServerZoneInstance
        {
            get;
// do not change this, required by the engine
#if GAME
			set;
#endif
        }

        public virtual void ServerInitialize()
        {
        }

        public void ServerUpdate(double fixedTimeStepSeconds)
        {
        }

        protected static TProtoScript GetScript<TProtoScript>()
            where TProtoScript : class, IZoneScript, new()
        {
            return Api.GetProtoEntity<TProtoScript>();
        }

        protected sealed override void PrepareProto()
        {
            if (IsClient)
            {
                return;
            }

            var scripts = new ZoneScripts();
            this.PrepareZone(scripts);
            this.AttachedScripts = scripts.ToReadOnly();

            // register zone scripts
            foreach (var zoneScript in this.AttachedScripts)
            {
                zoneScript.ServerRegisterZone(this);
            }
        }

        protected abstract void PrepareZone(ZoneScripts scripts);
    }
}