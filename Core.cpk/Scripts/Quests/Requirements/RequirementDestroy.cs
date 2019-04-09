namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RequirementDestroy : QuestRequirementWithList<IProtoStaticWorldObject>
    {
        public const string DescriptionFormat = "Destroy: {0}";

        private RequirementDestroy(
            IReadOnlyList<IProtoStaticWorldObject> list,
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

        public static RequirementDestroy Require<TProtoStaticWorldObject>(
            ushort count = 1,
            string description = null)
            where TProtoStaticWorldObject : class, IProtoStaticWorldObject
        {
            var list = Api.FindProtoEntities<TProtoStaticWorldObject>();
            return Require(list, count, description);
        }

        public static RequirementDestroy Require(
            IReadOnlyList<IProtoStaticWorldObject> list,
            ushort count,
            string description)
        {
            Api.Assert(list.Count > 0, "The list is of the required to destroy object prototypes is empty");
            return new RequirementDestroy(list, count, description);
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                ServerStaticObjectDestroyObserver.StructureDestroyed += this.ObjectDestroyedHandler;
            }
            else
            {
                ServerStaticObjectDestroyObserver.StructureDestroyed -= this.ObjectDestroyedHandler;
            }
        }

        private void ObjectDestroyedHandler(ICharacter character, IStaticWorldObject worldObject)
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