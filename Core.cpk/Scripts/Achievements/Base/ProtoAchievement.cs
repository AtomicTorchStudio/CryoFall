namespace AtomicTorch.CBND.CoreMod.Achievements
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    [PrepareOrder(afterType: typeof(IProtoSkill))]
    [PrepareOrder(afterType: typeof(IProtoTile))]
    [PrepareOrder(afterType: typeof(TechNode))]
    [PrepareOrder(afterType: typeof(TechGroup))]
    [PrepareOrder(afterType: typeof(QuestsSystem))]
    public abstract class ProtoAchievement
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoGameObject
          <ILogicObject,
              TPrivateState,
              TPublicState,
              TClientState>,
          IProtoAchievement
        where TPrivateState : BasePrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        protected ProtoAchievement()
        {
        }

        public abstract string AchievementId { get; }

        public override double ClientUpdateIntervalSeconds => double.MaxValue;

        public sealed override string Name => this.AchievementId;

        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        public sealed override string ShortId => this.AchievementId;

        public IReadOnlyList<IPlayerTask> Tasks { get; private set; }

        protected abstract void PrepareAchievement(TasksList tasks);

        protected sealed override void PrepareProto()
        {
            if (IsClient)
            {
                return;
            }

            var tasks = new TasksList();

            this.PrepareAchievement(tasks);

            Api.Assert(tasks.Count >= 1,   "At least one task required for a achievement");
            Api.Assert(tasks.Count <= 256, "Max 256 tasks per achievement");

            foreach (var task in tasks)
            {
                task.TaskTarget = this;
            }

            this.Tasks = tasks;
        }

        protected class TasksList : List<IPlayerTask>
        {
            public TasksList() : base(capacity: 1)
            {
            }

            public new TasksList Add(IPlayerTask task)
            {
                base.Add(task);
                return this;
            }
        }
    }

    public abstract class ProtoAchievement
        : ProtoAchievement<
            EmptyPrivateState,
            EmptyPublicState,
            EmptyClientState>
    {
    }
}