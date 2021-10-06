namespace AtomicTorch.CBND.CoreMod.Systems
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Logging;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// Base class for character action states.
    /// </summary>
    public abstract class BaseActionState<TPublicActionState> : IActionState
        where TPublicActionState : BasePublicActionState, new()
    {
        public readonly ICharacter Character;

        protected readonly PlayerCharacterPrivateState CharacterPrivateState;

        protected readonly PlayerCharacterPublicState CharacterPublicState;

        private double progressPercents;

        protected BaseActionState(ICharacter character)
        {
            this.Character = character;
            this.CharacterPrivateState = PlayerCharacter.GetPrivateState(character);
            this.CharacterPublicState = PlayerCharacter.GetPublicState(character);
        }

        public event Action<double> ProgressPercentsChanged
        {
            add
            {
                Api.ValidateIsClient();
                this.progressPercentsChanged += value;
            }
            remove => this.progressPercentsChanged -= value;
        }

        // ReSharper disable once InconsistentNaming
        private event Action<double> progressPercentsChanged;

        public virtual bool IsBlockingActions => false;

        public virtual bool IsBlockingMovement => false;

        // For mods compatibility
        [Obsolete("Use IsBlockingMovement instead")]
        public bool IsBlocksMovement => this.IsBlockingMovement;

        public bool IsCancelled { get; private set; }

        public bool IsCancelledByServer { get; set; }

        public bool IsCompleted { get; private set; }

        public virtual bool IsDisplayingProgress => true;

        public double ProgressPercents
        {
            get => this.progressPercents;
            protected set
            {
                if (this.progressPercents == value)
                {
                    return;
                }

                this.progressPercents = value;
                this.progressPercentsChanged?.Invoke(this.progressPercents);
            }
        }

        public abstract IWorldObject TargetWorldObject { get; }

        /// <summary>
        /// Gets the logger instance.
        /// </summary>
        protected static ILogger Logger => Api.Logger;

        public void Cancel()
        {
            if (this.IsCompleted)
            {
                return;
            }

            this.SetCompleted(isCancelled: true);
            this.AbortAction();
        }

        public void OnStart()
        {
            var publicState = PlayerCharacter.GetPublicState(this.Character);
            var publicActionState = new TPublicActionState()
            {
                TargetWorldObject = this.TargetWorldObject
            };
            this.SetupPublicState(publicActionState);
            publicState.CurrentPublicActionState = publicActionState;
            this.SharedOnStart();
        }

        public abstract void SharedUpdate(double deltaTime);

        protected abstract void AbortAction();

        protected virtual void OnCompletedOrCancelled()
        {
        }

        protected void SetCompleted(bool isCancelled)
        {
            if (this.IsCompleted)
            {
                return;
            }

            this.IsCompleted = true;
            this.IsCancelled = isCancelled;
            this.OnCompleted();
        }

        protected virtual void SetupPublicState(TPublicActionState state)
        {
        }

        protected virtual void SharedOnStart()
        {
        }

        private void OnCompleted()
        {
            var publicState = PlayerCharacter.GetPublicState(this.Character);
            publicState.CurrentPublicActionState.IsCancelled = this.IsCancelled;
            publicState.CurrentPublicActionState = null;

            try
            {
                this.OnCompletedOrCancelled();
            }
            finally
            {
                if (Api.IsClient
                    && this.TargetWorldObject is not null)
                {
                    ClientTimersSystem.AddAction(
                        delaySeconds:
                        0, // invoke on the next frame because current action completion is not yet processed
                        () => ClientComponentObjectInteractionHelper.OnInteractionFinished(this.TargetWorldObject));
                }
            }
        }
    }
}