namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    public abstract class BasePlayerTaskWithDefaultState : BasePlayerTask<PlayerTaskState>
    {
        protected BasePlayerTaskWithDefaultState(string description) : base(description)
        {
        }
    }
}