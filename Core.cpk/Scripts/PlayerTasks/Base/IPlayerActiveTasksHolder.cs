namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using AtomicTorch.CBND.CoreMod.Systems.Quests;

    public interface IPlayerActiveTasksHolder
    {
        void OnActiveTaskCompletedStateChanged(ServerPlayerActiveTask activeTask);
    }
}