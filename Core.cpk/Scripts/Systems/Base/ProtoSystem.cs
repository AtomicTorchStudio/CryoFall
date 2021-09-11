namespace AtomicTorch.CBND.CoreMod.Systems
{
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data;

    [PrepareOrder(afterType: typeof(ProtoTrigger))]
    [PrepareOrder(afterType: typeof(RatesSynchronizationSystem))]
    public abstract class ProtoSystem<TSystem> : ProtoEntity
        where TSystem : ProtoSystem<TSystem>
    {
        public static TSystem Instance { get; private set; }

        protected sealed override void PrepareProto()
        {
            Instance = (TSystem)this;
            this.PrepareSystem();
        }

        protected virtual void PrepareSystem()
        {
        }
    }
}