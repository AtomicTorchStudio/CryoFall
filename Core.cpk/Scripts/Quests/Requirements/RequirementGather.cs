namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Resources;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RequirementGather : QuestRequirementWithList<IProtoObjectGatherable>
    {
        public const string DescriptionFormat = "Gather: {0}";

        public RequirementGather(
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

        public static RequirementGather Require<TProtoObjectGatherable>(
            ushort count = 1,
            string description = null)
            where TProtoObjectGatherable : class, IProtoObjectGatherable
        {
            var list = Api.FindProtoEntities<TProtoObjectGatherable>();
            return new RequirementGather(list, count, description);
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
            if (context == null)
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