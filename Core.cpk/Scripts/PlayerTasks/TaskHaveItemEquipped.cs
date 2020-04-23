namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskHaveItemEquipped : BasePlayerTaskWithListAndCount<IProtoItemEquipment>
    {
        public const string DescriptionFormat = "Equip: {0}";

        private ServerWrappedTriggerTimeInterval serverUpdater;

        private TaskHaveItemEquipped(
            IReadOnlyList<IProtoItemEquipment> protoItems,
            string description)
            : base(protoItems,
                   count: 1,
                   description)
        {
        }

        public override bool IsReversible
            => true; // equipped item can be unequipped

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.ListNames);

        public static TaskHaveItemEquipped Require<TProtoItem>(string description = null)
            where TProtoItem : class, IProtoItemEquipment
        {
            var protoItems = Api.FindProtoEntities<TProtoItem>();
            return new TaskHaveItemEquipped(protoItems, description);
        }

        public static TaskHaveItemEquipped Require(
            IReadOnlyList<IProtoItemEquipment> protoItems,
            string description = null)
        {
            return new TaskHaveItemEquipped(protoItems, description);
        }

        public override ITextureResource ClientCreateIcon()
        {
            return this.List.Count == 1
                       ? this.List[0].Icon
                       : null;
        }

        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskStateWithCount state)
        {
            var containerEquipment = character.SharedGetPlayerContainerEquipment();

            if (this.List.Count == 1)
            {
                var requiredProtoItem = this.List[0];
                foreach (var item in containerEquipment.Items)
                {
                    if (item.ProtoItem == requiredProtoItem)
                    {
                        return true;
                    }
                }
            }
            else
            {
                foreach (var item in containerEquipment.Items)
                {
                    if (this.List.Contains(item.ProtoItem))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                this.serverUpdater = new ServerWrappedTriggerTimeInterval(
                    this.ServerRefreshAllActiveContexts,
                    TimeSpan.FromSeconds(1),
                    "QuestRequirement.HaveItemEquipped");
            }
            else
            {
                this.serverUpdater.Dispose();
                this.serverUpdater = null;
            }
        }
    }
}