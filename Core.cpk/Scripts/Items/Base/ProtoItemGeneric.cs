namespace AtomicTorch.CBND.CoreMod.Items
{
    using AtomicTorch.CBND.GameApi.Data.State;

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
        where TPrivateState : ItemPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        /// <summary>
        /// By default max stack size is "Medium".
        /// </summary>
        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        protected override string GenerateIconPath()
        {
            return "Items/Generic/" + this.GetType().Name;
        }
    }

    /// <summary>
    /// Item prototype for generic items.
    /// </summary>
    public abstract class ProtoItemGeneric
        : ProtoItemGeneric
            <ItemPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}