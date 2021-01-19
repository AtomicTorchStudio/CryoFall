namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Systems.FishingSystem;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskCatchFish : BasePlayerTaskWithListAndCount<IProtoItemFish>
    {
        // not used as we're using this as a "catch any fish"
        [NotLocalizable]
        public const string DescriptionFormat = "Catch fish: {0}";

        public TaskCatchFish(
            IReadOnlyList<IProtoItemFish> list,
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

        public static TaskCatchFish Require<TProtoItemFish>(
            ushort count = 1,
            string description = null)
            where TProtoItemFish : class, IProtoItemFish
        {
            var list = Api.FindProtoEntities<TProtoItemFish>();
            return Require(list, count, description);
        }

        public static TaskCatchFish Require<TProtoItemFish>(
            IReadOnlyList<TProtoItemFish> list,
            ushort count = 1,
            string description = null)
            where TProtoItemFish : class, IProtoItemFish
        {
            return new(list, count, description);
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
                FishingSystem.ServerFishCaught += this.FishCaughtHandler;
            }
            else
            {
                FishingSystem.ServerFishCaught -= this.FishCaughtHandler;
            }
        }

        private void FishCaughtHandler(ICharacter character, IItem item, float sizeValue)
        {
            var context = this.GetActiveContext(character, out var state);
            if (context is null)
            {
                return;
            }

            if (!this.List.Contains(item.ProtoItem))
            {
                return;
            }

            state.SetCountCurrent(state.CountCurrent + 1, this.RequiredCount);
            context.Refresh();
        }
    }
}