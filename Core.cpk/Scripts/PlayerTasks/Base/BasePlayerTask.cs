namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class BasePlayerTask<TState> : IPlayerTask
        where TState : PlayerTaskState, new()
    {
        private readonly Dictionary<ICharacter, ServerPlayerActiveTask> activeTasks
            = new Dictionary<ICharacter, ServerPlayerActiveTask>();

        private readonly string description;

        private Lazy<ITextureResource> cachedIcon;

        private IProtoLogicObject taskTarget;

        protected BasePlayerTask(string description)
        {
            this.description = description;
        }

        public string Description => this.description
                                     ?? this.AutoDescription;

        public ITextureResource Icon
        {
            get
            {
                if (this.cachedIcon != null)
                {
                    return this.cachedIcon.Value;
                }

                var icon = this.ClientCreateIcon();
                if (TextureResource.NoTexture.Equals(icon))
                {
                    icon = null;
                }

                this.cachedIcon = new Lazy<ITextureResource>(() => icon);

                return this.cachedIcon.Value;
            }
        }

        public abstract bool IsReversible { get; }

        public IProtoLogicObject TaskTarget
        {
            get => this.taskTarget;
            set
            {
                if (this.taskTarget != null)
                {
                    throw new Exception("Target object is already associated with this task");
                }

                this.taskTarget = value;
            }
        }

        protected virtual string AutoDescription { get; }

        public virtual ITextureResource ClientCreateIcon() => null;

        public PlayerTaskState CreateState()
        {
            return new TState();
        }

        public bool IsStateTypeMatch(PlayerTaskState state)
        {
            return state is TState;
        }

        public void ServerRefreshIsCompleted(ICharacter character, PlayerTaskState state)
        {
            if (state.IsCompleted
                && !this.IsReversible)
            {
                // was already completed - don't change
                return;
            }

            var wasCompleted = state.IsCompleted;
            state.IsCompleted = this.ServerIsCompleted(character, (TState)state);
            if (wasCompleted == state.IsCompleted)
            {
                return;
            }

            if (!wasCompleted)
            {
                Api.Logger.Important("Task completed: " + this, character);
            }
            else
            {
                Api.Logger.Important("Reversible task became incomplete: " + this, character);
            }
        }

        public void ServerRegisterOrUnregisterContext(ServerPlayerActiveTask context)
        {
            if (context.IsActive)
            {
                this.activeTasks.Add(context.Character, context);
                if (this.activeTasks.Count == 1)
                {
                    this.SetTriggerActive(isActive: true);
                }
            }
            else
            {
                this.activeTasks.Remove(context.Character);
                if (this.activeTasks.Count == 0)
                {
                    this.SetTriggerActive(isActive: false);
                }
            }
        }

        public override string ToString()
        {
            return this.GetType().Name + " - " + this.AutoDescription;
        }

        public IPlayerTask WithIcon(ITextureResource icon)
        {
            this.cachedIcon = new Lazy<ITextureResource>(() => icon);
            return this;
        }

        protected ServerPlayerActiveTask GetActiveContext(ICharacter character, out TState state)
        {
            if (character == null)
            {
                state = default;
                return null;
            }

            if (this.activeTasks.TryGetValue(character, out var result))
            {
                state = (TState)result.PlayerTaskState;
                return result;
            }

            state = default;
            return null;
        }

        protected abstract bool ServerIsCompleted(ICharacter character, TState state);

        protected void ServerRefreshAllActiveContexts()
        {
            if (this.activeTasks.Count == 0)
            {
                return;
            }

            using var tempList = Api.Shared.GetTempList<
                KeyValuePair<ICharacter, ServerPlayerActiveTask>>();
            // make a copy of the list (as it could be modified during the enumeration)
            // and process it
            tempList.AddRange(this.activeTasks);
            foreach (var pair in tempList.AsList())
            {
                if (!pair.Key.ServerIsOnline)
                {
                    continue;
                }

                pair.Value.Refresh();
            }
        }

        protected virtual void SetTriggerActive(bool isActive)
        {
        }
    }
}