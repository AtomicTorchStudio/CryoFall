namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public interface IObjectWithAccessModePrivateState : IPrivateState
    {
        WorldObjectDirectAccessMode DirectAccessMode { get; set; }

        WorldObjectFactionAccessModes FactionAccessMode { get; set; }
    }
}