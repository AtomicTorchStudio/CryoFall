namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Resources;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskGather : BasePlayerTaskWithListAndCount<IProtoObjectGatherable>
    {
        public const string DescriptionFormat = "Gather: {0}";

        private TaskGather(
            IReadOnlyList<IProtoObjectGatherable> list,
            ushort count,
            string description)
            : base(list,
                   count,
                   description)
        {
        }

        public override bool IsReversible => false;

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.ListNames);

        public static TaskGather Require<TProtoObjectGatherable>(
            ushort count = 1,
            string description = null)
            where TProtoObjectGatherable : class, IProtoObjectGatherable
        {
            var list = Api.FindProtoEntities<TProtoObjectGatherable>();
            return Require(list, count, description);
        }

        public static TaskGather Require(
            IReadOnlyList<IProtoObjectGatherable> list,
            ushort count,
            string description)
        {
            return new TaskGather(list, count, description);
        }

        public override ITextureResource ClientCreateIcon()
        {
            return this.List.Count == 1
                       ? this.List[0].Icon
                       : null;
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                GatheringSystem.ServerOnGather += this.ServerOnGatherHandler;
            }
            else
            {
                GatheringSystem.ServerOnGather -= this.ServerOnGatherHandler;
            }
        }

        private void ServerOnGatherHandler(ICharacter character, IStaticWorldObject worldObject)
        {
            var context = this.GetActiveContext(character, out var state);
            if (context is null)
            {
                return;
            }

            if (!this.List.Contains(worldObject.ProtoStaticWorldObject))
            {
                return;
            }

            state.SetCountCurrent(state.CountCurrent + 1, this.RequiredCount);
            context.Refresh();
        }
    }
}