namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class QuestRequirementWithList<TProtoEntity, TState>
        : QuestRequirement<TState>, IQuestRequirementWithCount
        where TProtoEntity : IProtoEntity
        where TState : QuestRequirementStateWithCount, new()
    {
        protected QuestRequirementWithList(
            IReadOnlyList<TProtoEntity> list,
            ushort count,
            string description)
            : base(description)
        {
            this.List = list;
            Api.Assert(this.List.Count > 0,
                       "The list for requirement " + this.GetType().Name + " is empty");

            this.RequiredCount = count;
            Api.Assert(this.RequiredCount > 0,
                       "The required count for requirement " + this.GetType().Name + " is 0");
        }

        public ushort RequiredCount { get; }

        protected IReadOnlyList<TProtoEntity> List { get; }

        protected AutoStringBuilder ListNames
        {
            get
            {
                if (this.List.Count == 1)
                {
                    return this.List[0].Name;
                }

                var result = this.List.Select(i => i.Name)
                                 .Take(3)
                                 .GetJoinedString("/");
                if (this.List.Count > 3)
                {
                    result += "...";
                }

                return result;
            }
        }

        protected override bool ServerIsSatisfied(ICharacter character, TState state)
        {
            return state.CountCurrent >= this.RequiredCount;
        }
    }

    public abstract class QuestRequirementWithList<TProtoEntity>
        : QuestRequirementWithList<TProtoEntity, QuestRequirementStateWithCount>
        where TProtoEntity : IProtoEntity
    {
        protected QuestRequirementWithList(
            IReadOnlyList<TProtoEntity> list,
            ushort count,
            string description)
            : base(
                list,
                count,
                description)
        {
        }
    }
}