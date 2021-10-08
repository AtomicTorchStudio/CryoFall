namespace AtomicTorch.CBND.CoreMod.Systems.FishingBaitReloadingSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Systems.FishingSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class FishingBaitReloadingSystem
        : ProtoSystem<FishingBaitReloadingSystem>
    {
        public const string NotificationNoBait_Message
            = "You don't have any bait in your inventory to use for fishing.";

        public const string NotificationNoBait_Title = "No bait";

        private static readonly Lazy<IReadOnlyCollection<IProtoItemFishingBait>> AllBaitProtos
            = new(() => new HashSet<IProtoItemFishingBait>(Api.FindProtoEntities<IProtoItemFishingBait>()));

        public static void ClientTrySwitchBaitType()
        {
            var character = Api.Client.Characters.CurrentPlayerCharacter;

            var itemRod = character.SharedGetPlayerSelectedHotbarItem();
            if (itemRod?.ProtoItem is not IProtoItemToolFishing protoRod)
            {
                // no selected rod to refill
                return;
            }

            var itemPrivateState = itemRod.GetPublicState<ItemFishingRodPublicState>();
            var compatibleBaitGroups = SharedGetCompatibleBaitGroups(character);
            if (compatibleBaitGroups.Count == 0)
            {
                NotificationSystem.ClientShowNotification(
                    NotificationNoBait_Title,
                    NotificationNoBait_Message,
                    NotificationColor.Bad,
                    protoRod.Icon,
                    playSound: false);
                return;
            }

            IProtoItemFishingBait selectedProtoBait;

            if (itemPrivateState.CurrentProtoBait is null
                || !SharedGetCompatibleBaitGroups(character)
                    .Any(c => ReferenceEquals(c.Key, itemPrivateState.CurrentProtoBait)))
            {
                // no bait selected or no bait of the selected type available
                selectedProtoBait = SharedFindNextBaitGroup(compatibleBaitGroups,
                                                            currentProtoBait: null)?.Key;
            }
            else // if already has a bait
            {
                selectedProtoBait = SharedFindNextBaitGroup(compatibleBaitGroups,
                                                            itemPrivateState.CurrentProtoBait)?.Key;
            }

            if (ReferenceEquals(selectedProtoBait, itemPrivateState.CurrentProtoBait))
            {
                return;
            }

            var request = new FishingBaitRefillRequest(character, itemRod, selectedProtoBait);
            Instance.CallServer(_ => _.ServerRemote_SelectBaitType(request));
            SharedSelectBaitItemType(request);
        }

        private static IEnumerable<IItem> SharedFindBaitItems(
            ICharacter character,
            IProtoItemFishingBait requestProtoItemBait)
        {
            var baitGroups = SharedGetCompatibleBaitGroups(character);
            foreach (var baitGroup in baitGroups)
            {
                if (ReferenceEquals(baitGroup.Key, requestProtoItemBait))
                {
                    return baitGroup;
                }
            }

            return Array.Empty<IItem>();
        }

        private static IGrouping<IProtoItemFishingBait, IItem> SharedFindNextBaitGroup(
            List<IGrouping<IProtoItemFishingBait, IItem>> existingGroups,
            IProtoItemFishingBait currentProtoBait)
        {
            var baitIndex = -1;
            using var allBaitTempList = Api.Shared.WrapInTempList(AllBaitProtos.Value);
            var allBait = allBaitTempList.AsList();
            for (var index = 0; index < allBait.Count; index++)
            {
                var compatibleItem = allBait[index];
                if (compatibleItem == currentProtoBait)
                {
                    // found current proto item, select next item prototype
                    baitIndex = index;
                    break;
                }
            }

            if (baitIndex < 0)
            {
                baitIndex = -1;
            }

            // try to find next available bait
            do
            {
                baitIndex++;
                if (baitIndex >= allBait.Count)
                {
                    // unload bait
                    return null;
                }

                var requiredBaitType = allBait[baitIndex];

                foreach (var availableBait in existingGroups)
                {
                    if (availableBait.Key == requiredBaitType)
                    {
                        // found required bait
                        return availableBait;
                    }
                }
            }
            while (true);
        }

        private static List<IGrouping<IProtoItemFishingBait, IItem>> SharedGetCompatibleBaitGroups(ICharacter character)
        {
            var allBaitProtos = AllBaitProtos.Value;
            return character.ProtoCharacter
                            .SharedEnumerateAllContainers(character, includeEquipmentContainer: false)
                            .SelectMany(c => c.Items)
                            .Where(i => allBaitProtos.Contains(i.ProtoGameObject))
                            .GroupBy(a => (IProtoItemFishingBait)a.ProtoItem)
                            .ToList();
        }

        private static void SharedSelectBaitItemType(FishingBaitRefillRequest request)
        {
            try
            {
                SharedValidateRequest(request);
            }
            catch (Exception ex)
            {
                Logger.Warning(ex.Message, request.Character);
                return;
            }

            var character = request.Character;
            var itemFishingRod = request.ItemFishingRod;
            var itemFishingRodPublicState = itemFishingRod.GetPublicState<ItemFishingRodPublicState>();

            var selectedProtoBait = request.ProtoItemBait;
            if (selectedProtoBait is null)
            {
                itemFishingRodPublicState.CurrentProtoBait = null;
                return;
            }

            var selectedBaitGroup = SharedGetCompatibleBaitGroups(character)
                .FirstOrDefault(g => g.Key == selectedProtoBait);

            if (selectedBaitGroup is null)
            {
                Logger.Warning(
                    $"Bait refilling impossible: {itemFishingRod} - no bait of the required type ({selectedProtoBait})",
                    character);

                selectedProtoBait = null;
            }

            itemFishingRodPublicState.CurrentProtoBait = selectedProtoBait;
        }

        private static void SharedValidateRequest(FishingBaitRefillRequest request)
        {
            var itemFishingRod = request.ItemFishingRod;
            var itemFishingRodPublicState = itemFishingRod.GetPublicState<ItemFishingRodPublicState>();
            var currentProtoBait = itemFishingRodPublicState.CurrentProtoBait;

            if (itemFishingRod != request.Character.SharedGetPlayerSelectedHotbarItem())
            {
                throw new Exception("The item is not selected");
            }

            if (itemFishingRod.ProtoItem is not IProtoItemToolFishing)
            {
                throw new Exception("The item must be a fishing rod");
            }

            if (ReferenceEquals(request.ProtoItemBait, currentProtoBait))
            {
                throw new Exception("No bait switching needed");
            }

            if (PlayerCharacter.GetPrivateState(request.Character)
                               .CurrentActionState is FishingActionState)
            {
                throw new Exception("Cannot switch bait type while fishing");
            }

            if (request.ProtoItemBait is null)
            {
                return;
            }

            var itemsToConsume = SharedFindBaitItems(request.Character, request.ProtoItemBait);
            if (!itemsToConsume.Any())
            {
                throw new Exception("No items to select for bait");
            }
        }

        private void ServerRemote_SelectBaitType(FishingBaitRefillRequest request)
        {
            SharedSelectBaitItemType(request);
        }

        private readonly struct FishingBaitRefillRequest : IRemoteCallParameter
        {
            public readonly ICharacter Character;

            public readonly IItem ItemFishingRod;

            public readonly IProtoItemFishingBait ProtoItemBait;

            public FishingBaitRefillRequest(
                ICharacter character,
                IItem itemFishingRod,
                IProtoItemFishingBait protoItemBait)
            {
                this.Character = character;
                this.ItemFishingRod = itemFishingRod;
                this.ProtoItemBait = protoItemBait;
            }
        }
    }
}