namespace AtomicTorch.CBND.CoreMod.Systems
{
    using AtomicTorch.CBND.GameApi.Data;

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