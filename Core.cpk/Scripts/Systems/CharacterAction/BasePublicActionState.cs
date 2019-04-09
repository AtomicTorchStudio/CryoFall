namespace AtomicTorch.CBND.CoreMod.Systems
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class BasePublicActionState : BaseNetObject
    {
        [SyncToClient(receivers: SyncToClientReceivers.ScopePlayers)]
        public bool IsCancelled { get; set; }

        [SyncToClient(receivers: SyncToClientReceivers.ScopePlayers)]
        public IWorldObject TargetWorldObject { get; set; }

        [TempOnly]
        protected ICharacter Character { get; private set; }

        public void InvokeClientOnCompleted()
        {
            this.ClientOnCompleted();
        }

        public void InvokeClientOnStart(ICharacter character)
        {
            this.Character = character;
            this.ClientOnStart();
        }

        protected abstract void ClientOnCompleted();

        protected abstract void ClientOnStart();
    }
}