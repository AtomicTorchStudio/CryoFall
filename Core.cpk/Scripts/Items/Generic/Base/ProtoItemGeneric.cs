namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;

    /// <summary>
    /// Item prototype for generic items.
    /// </summary>
    public abstract class ProtoItemGeneric
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItem
            <TPrivateState,
                TPublicState,
                TClientState>
        where TPrivateState : BasePrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        protected ProtoItemGeneric()
        {
            this.Icon = new TextureResource("Items/Generic/" + this.GetType().Name);
        }

        public override ITextureResource Icon { get; }

        /// <summary>
        /// By default max stack size is "Medium".
        /// </summary>
        public override ushort MaxItemsPerStack => ItemStackSize.Medium;
    }

    /// <summary>
    /// Item prototype for generic items.
    /// </summary>
    public abstract class ProtoItemGeneric
        : ProtoItemGeneric
            <EmptyPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}