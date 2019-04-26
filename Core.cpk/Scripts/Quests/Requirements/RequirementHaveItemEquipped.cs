namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RequirementHaveItemEquipped : QuestRequirementWithList<IProtoItemEquipment>
    {
        public const string DescriptionFormat = "Equip: {0}";

        private ServerWrappedTriggerTimeInterval serverUpdater;

        private RequirementHaveItemEquipped(
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

        public static RequirementHaveItemEquipped Require<TProtoItem>(string description = null)
            where TProtoItem : class, IProtoItemEquipment
        {
            var protoItems = Api.FindProtoEntities<TProtoItem>();
            return new RequirementHaveItemEquipped(protoItems, description);
        }

        public static RequirementHaveItemEquipped Require(
            IReadOnlyList<IProtoItemEquipment> protoItems,
            string description = null)
        {
            return new RequirementHaveItemEquipped(protoItems, description);
        }

        protected override bool ServerIsSatisfied(ICharacter character, QuestRequirementStateWithCount state)
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