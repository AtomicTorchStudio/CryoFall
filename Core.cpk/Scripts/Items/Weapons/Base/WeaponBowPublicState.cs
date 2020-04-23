namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class WeaponBowPublicState : BasePublicState
    {
        [SyncToClient(isAllowClientSideModification: true,
                      receivers: SyncToClientReceivers.ScopePlayers)]
        public bool IsReady { get; set; }
    }
}