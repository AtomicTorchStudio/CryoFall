namespace AtomicTorch.CBND.CoreMod.Items.Fishing.Base
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ItemFishingRodPublicState : BasePublicState
    {
        [SyncToClient(isAllowClientSideModification: true, isSendChanges: false)]
        public IProtoItemFishingBait CurrentProtoBait { get; set; }
    }
}