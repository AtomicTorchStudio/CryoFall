namespace AtomicTorch.CBND.CoreMod.Systems
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class BasePublicActionState : BaseNetObject
    {
        [SyncToClient(receivers: SyncToClientReceivers.ScopePlayers)]
        public bool IsCancelled { get; set; }

        [SyncToClient(receivers: SyncToClientReceivers.ScopePlayers)]
        public IWorldObject TargetWorldObject { get; set; }

        [TempOnly]
        protected ICharacter Character { get; private set; }

        public void InvokeClientDeinitialize()
        {
            try
            {
                this.ClientDeinitialize();
            }
            catch (Exception ex)
            {
                Api.Logger.Exception(ex);
            }
        }

        public void InvokeClientOnCompleted()
        {
            this.InvokeClientDeinitialize();

            try
            {
                this.ClientOnCompleted();
            }
            catch (Exception ex)
            {
                Api.Logger.Exception(ex);
            }
        }

        public void InvokeClientOnStart(ICharacter character)
        {
            this.Character = character;
            try
            {
                this.ClientOnStart();
            }
            catch (Exception ex)
            {
                Api.Logger.Exception(ex);
            }
        }

        protected virtual void ClientDeinitialize()
        {
        }

        protected abstract void ClientOnCompleted();

        protected abstract void ClientOnStart();
    }
}