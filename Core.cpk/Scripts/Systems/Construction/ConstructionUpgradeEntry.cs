namespace AtomicTorch.CBND.CoreMod.Systems.Construction
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConstructionUpgradeEntry
        : IConstructionUpgradeEntryReadOnly
    {
        private readonly InputItems requiredItems;

        public ConstructionUpgradeEntry(IProtoObjectStructure protoStructure)
        {
            this.ProtoStructure = protoStructure;
            this.requiredItems = new InputItems();
            this.RequiredItems = this.requiredItems.AsReadOnly();
        }

        public IProtoObjectStructure ProtoStructure { get; }

        public IReadOnlyList<ProtoItemWithCount> RequiredItems { get; }

        public ConstructionUpgradeEntry AddRequiredItem<TProtoItem>(ushort count = 1)
            where TProtoItem : class, IProtoItem, new()
        {
            this.requiredItems.Add<TProtoItem>(count);
            return this;
        }

        public void ApplyRates(byte multiplier)
        {
            this.requiredItems.ApplyRates(multiplier);
        }

        public bool CheckRequirementsSatisfied(ICharacter character)
        {
            if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return true;
            }

            if (!this.ProtoStructure.SharedIsTechUnlocked(character))
            {
                // the tech is locked
                return false;
            }

            foreach (var requiredItem in this.RequiredItems)
            {
                if (!character.ContainsItemsOfType(requiredItem.ProtoItem, requiredItem.Count))
                {
                    // some item is not available
                    return false;
                }
            }

            return true;
        }

        public void ServerDestroyRequiredItems(ICharacter character)
        {
            if (CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                Api.Logger.Important(character + " is in the admin mode - free construction is allowed.");
                return;
            }

            // assume all the validation has been done before this action
            var serverItemsService = Api.Server.Items;
            var itemsChangedCount = new Dictionary<IProtoItem, int>();

            foreach (var requiredItem in this.RequiredItems)
            {
                serverItemsService.DestroyItemsOfType(
                    character,
                    requiredItem.ProtoItem,
                    requiredItem.Count,
                    out _);
                itemsChangedCount[requiredItem.ProtoItem] = -requiredItem.Count;
            }

            NotificationSystem.ServerSendItemsNotification(character, itemsChangedCount);
        }
    }
}