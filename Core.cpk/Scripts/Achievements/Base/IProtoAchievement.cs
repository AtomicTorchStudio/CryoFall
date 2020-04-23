namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.GameApi.Data.Logic;

    public interface IProtoAchievement : IProtoLogicObject
    {
        string AchievementId { get; }

        IReadOnlyList<IPlayerTask> Tasks { get; }
    }
}