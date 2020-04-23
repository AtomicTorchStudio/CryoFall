namespace AtomicTorch.CBND.CoreMod.Systems.Quests
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    /// <summary>
    /// Please note - this a non-serializable class.
    /// </summary>
    public sealed class ServerPlayerActiveTask
    {
        private bool isActive;

        public ServerPlayerActiveTask(
            IPlayerTask playerTask,
            PlayerTaskState playerTaskState,
            IPlayerActiveTasksHolder playerTaskTarget,
            ICharacter character)
        {
            this.PlayerTask = playerTask;
            this.PlayerTaskState = playerTaskState;
            this.PlayerTaskTarget = playerTaskTarget;
            this.Character = character;
        }

        public ICharacter Character { get; }

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;
                //Api.Logger.Important("Active quest requirement status changed: " + this, this.Character);
                this.PlayerTask.ServerRegisterOrUnregisterContext(this);
            }
        }

        public IPlayerTask PlayerTask { get; }

        public PlayerTaskState PlayerTaskState { get; }

        public IPlayerActiveTasksHolder PlayerTaskTarget { get; }

        public void Refresh()
        {
            var task = this.PlayerTask;
            var state = this.PlayerTaskState;

            if (state.IsCompleted
                && !task.IsReversible)
            {
                return;
            }

            var wasSatisfied = state.IsCompleted;

            task.ServerRefreshIsCompleted(this.Character, state);

            if (state.IsCompleted != wasSatisfied)
            {
                this.PlayerTaskTarget.OnActiveTaskCompletedStateChanged(this);
            }
        }

        public void SetCompleted()
        {
            var state = this.PlayerTaskState;
            if (state.IsCompleted)
            {
                return;
            }

            state.IsCompleted = true;

            this.PlayerTaskTarget.OnActiveTaskCompletedStateChanged(this);
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}, {2}: {3}, {4}: {5}",
                                 nameof(this.Character),
                                 this.Character,
                                 nameof(this.PlayerTask),
                                 this.PlayerTask,
                                 nameof(this.IsActive),
                                 this.IsActive);
        }
    }
}