namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public interface IObjectWithOwnersPrivateState : IPrivateState
    {
        NetworkSyncList<string> Owners { get; set; }
    }
}