namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public interface IObjectWithAccessModePrivateState : IPrivateState
    {
        WorldObjectAccessMode AccessMode { get; set; }
    }
}