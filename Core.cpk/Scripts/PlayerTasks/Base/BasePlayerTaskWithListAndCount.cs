namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class BasePlayerTaskWithListAndCount<TProtoEntity, TState>
        : BasePlayerTaskWithCount<TState>
        where TProtoEntity : IProtoEntity
        where TState : PlayerTaskStateWithCount, new()
    {
        protected BasePlayerTaskWithListAndCount(
            IReadOnlyList<TProtoEntity> list,
            ushort count,
            string description)
            : base(count, description)
        {
            this.List = list;
            Api.Assert(this.List.Count > 0,
                       "The list for requirement " + this.GetType().Name + " is empty");
        }

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
    }

    public abstract class BasePlayerTaskWithListAndCount<TProtoEntity>
        : BasePlayerTaskWithListAndCount<TProtoEntity, PlayerTaskStateWithCount>
        where TProtoEntity : IProtoEntity
    {
        protected BasePlayerTaskWithListAndCount(
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