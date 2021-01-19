namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    [PrepareOrder(afterType: typeof(IProtoStaticWorldObject))]
    [PrepareOrder(afterType: typeof(IProtoSkill))]
    [PrepareOrder(afterType: typeof(IProtoTile))]
    [PrepareOrder(afterType: typeof(TechNode))]
    [PrepareOrder(afterType: typeof(TechGroup))]
    public abstract class ProtoQuest
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoGameObject
          <ILogicObject,
              TPrivateState,
              TPublicState,
              TClientState>,
          IProtoQuest
        where TPrivateState : BasePrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        private HintsList hintsList;

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        protected ProtoQuest()
        {
            var thisType = this.GetType();
            var name = thisType.Name;
            if (name.StartsWith("Quest"))
            {
                name = name.Substring("Quest".Length);
            }

            this.ShortId = name;

            var protoQuest = typeof(ProtoQuest);
            var iconPath = thisType.FullName
                                   // remove namespace of base class
                                   .Substring(protoQuest.FullName.Length - protoQuest.Name.Length);

            var icon = new TextureResource("Quests/" + iconPath.Replace('.', '/'));
            if (!Api.Shared.IsFileExists(icon))
            {
                // TODO: restore this when we will implement the quest icons
                //Logger.Warning("Icon not found: " + icon.FullPath + ", using default generic icon.");
                // icon not found - fallback to default texture
                icon = new TextureResource("Quests/Unknown.png");
            }

            this.Icon = icon;
        }

        public override double ClientUpdateIntervalSeconds => double.MaxValue;

        public abstract string Description { get; }

        public virtual string Hints => null;

        public ITextureResource Icon { get; }

        public IReadOnlyList<IProtoQuest> Prerequisites { get; private set; }

        public abstract ushort RewardLearningPoints { get; }

        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        public override string ShortId { get; }

        public IReadOnlyList<IPlayerTask> Tasks { get; private set; }

        string IProtoQuest.Hints => this.hintsList.ToString();

        protected sealed override void PrepareProto()
        {
            var tasks = new TasksList();
            var prerequisites = new QuestsList();

            // ReSharper disable once LocalVariableHidesMember
            var hintsList = new HintsList();
            if (!string.IsNullOrEmpty(this.Hints))
            {
                hintsList.Add(this.Hints);
            }

            this.PrepareQuest(prerequisites, tasks, hintsList);

            Api.Assert(tasks.Count >= 1,           "At least one task required for a quest");
            Api.Assert(tasks.Count <= 256,         "Max 256 tasks per quest");
            Api.Assert(prerequisites.Count <= 256, "Max 256 prerequisites per quest");

            foreach (var task in tasks)
            {
                task.TaskTarget = this;
            }

            this.Tasks = tasks;
            this.Prerequisites = prerequisites;

            this.hintsList = hintsList;
        }

        protected abstract void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints);

        protected class QuestsList : List<IProtoQuest>, IReadOnlyQuestsList
        {
            public QuestsList() : base(capacity: 0)
            {
            }

            public QuestsList Add<TQuestType>()
                where TQuestType : IProtoQuest, new()
            {
                this.Add(Api.GetProtoEntity<TQuestType>());
                return this;
            }

            public QuestsList AddAll<TQuestType>()
                where TQuestType : class, IProtoQuest
            {
                var protos = Api.FindProtoEntities<TQuestType>();
                if (protos.Count == 0)
                {
                    throw new Exception("Cannot find proto classes implementing " + typeof(TQuestType).Name);
                }

                this.AddRange(protos);
                return this;
            }
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

    public abstract class ProtoQuest
        : ProtoQuest<
            EmptyPrivateState,
            EmptyPublicState,
            EmptyClientState>
    {
    }
}