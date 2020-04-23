namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    public interface IPlayerTaskWithCount : IPlayerTask
    {
        ushort RequiredCount { get; }
    }
}