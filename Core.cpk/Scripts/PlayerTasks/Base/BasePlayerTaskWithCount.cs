namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class BasePlayerTaskWithCount<TState>
        : BasePlayerTask<TState>, IPlayerTaskWithCount
        where TState : PlayerTaskStateWithCount, new()
    {
        protected BasePlayerTaskWithCount(
            ushort count,
            string description)
            : base(description)
        {
            this.RequiredCount = count;
            Api.Assert(this.RequiredCount > 0,
                       "The required count for requirement " + this.GetType().Name + " is 0");
        }

        public ushort RequiredCount { get; }

        protected override bool ServerIsCompleted(ICharacter character, TState state)
        {
            return state.CountCurrent >= this.RequiredCount;
        }
    }
}