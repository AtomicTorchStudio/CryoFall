namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Objects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskUseObject : BasePlayerTaskWithListAndCount<IProtoWorldObject>
    {
        public TaskUseObject(
            IReadOnlyList<IProtoWorldObject> list,
            ushort count,
            string description)
            : base(list,
                   count,
                   description)
        {
        }

        public override bool IsReversible => false;

        protected override string AutoDescription
            => string.Format(TaskUseItem.DescriptionFormat, this.ListNames);

        public static TaskUseObject Require<TProtoWorldObject>(
            ushort count = 1,
            string description = null)
            where TProtoWorldObject : class, IProtoWorldObject
        {
            var list = Api.FindProtoEntities<TProtoWorldObject>();
            return Require(list, count, description);
        }

        public static TaskUseObject Require<TProtoWorldObject>(
            IReadOnlyList<TProtoWorldObject> list,
            ushort count = 1,
            string description = null)
            where TProtoWorldObject : class, IProtoWorldObject
        {
            return new(list, count, description);
        }

        public override ITextureResource ClientCreateIcon()
        {
            return this.List.Count == 1
                       ? (this.List[0] as IProtoStaticWorldObject)?.Icon
                       : null;
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                ServerObjectUseObserver.ObjectUsed += this.ObjectUsedHandler;
            }
            else
            {
                ServerObjectUseObserver.ObjectUsed -= this.ObjectUsedHandler;
            }
        }

        private void ObjectUsedHandler(ICharacter character, IWorldObject worldObject)
        {
            var context = this.GetActiveContext(character, out var state);
            if (context is null)
            {
                return;
            }

            if (!this.List.Contains(worldObject.ProtoWorldObject))
            {
                return;
            }

            state.SetCountCurrent(state.CountCurrent + 1, this.RequiredCount);
            context.Refresh();
        }
    }
}