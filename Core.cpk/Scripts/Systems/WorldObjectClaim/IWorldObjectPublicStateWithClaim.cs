namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim
{
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;

    public interface IWorldObjectPublicStateWithClaim : IPublicState
    {
        [SyncToClient]
        [TempOnly]
        public ILogicObject WorldObjectClaim { get; set; }
    }
}