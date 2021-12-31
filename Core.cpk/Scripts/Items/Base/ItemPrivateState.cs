namespace AtomicTorch.CBND.CoreMod.Items
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ItemPrivateState : BasePrivateState
    {
        [SyncToClient]
        public string CreatedByPlayerName { get; set; }
    }
}