namespace AtomicTorch.CBND.CoreMod.Systems.WateringCanRefill
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Tools.WateringCans;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class WateringCanRefillSystem
        : ProtoActionSystem<
            WateringCanRefillSystem,
            WateringCanRefillRequest,
            WateringCanRefillActionState,
            WateringCanRefillActionState.PublicState>
    {
        /// <summary>
        /// 1 bottle will give 3 "charges" of watering can.
        /// </summary>
        public const byte BottleWaterAmount = 3;

        public const string NotificationCannotRefill_Message =
            "To refill, point on a well or have bottles with water in your inventory.";

        public const string NotificationCannotRefill_Title = "Cannot refill";

        /// <summary>
        /// Consume max 100 bottle per refill (it means it will always try to refill the watering can to maximum).
        /// bottles).
        /// </summary>
        private const int MaxBottlesToConsumePerRefill = 100;

        public override string Name => "Watering can refill system";

        protected override WateringCanRefillRequest ClientTryCreateRequest(ICharacter character)
        {
            var item = character.SharedGetPlayerSelectedHotbarItem();
            if (!(item?.ProtoGameObject is IProtoItemToolWateringCan))
            {
                // no watering can is selected
                return null;
            }

            var waterAmountNeeded = SharedGetWaterAmountNeeded(item);
            if (waterAmountNeeded <= 0)
            {
                // no need to refill
                return null;
            }

            IReadOnlyList<IItem> bottleItemsToConsume;

            // try to find a well object
            var worldObject = ClientComponentObjectInteractionHelper.MouseOverObject;
            if (worldObject != null
                && !(worldObject.ProtoWorldObject is ProtoObjectWell))
            {
                worldObject = null;
            }

            if (worldObject != null)
            {
                // no need to consume water bottles - will consume water from well
                bottleItemsToConsume = null;
            }
            else
            {
                // need to consume water from bottles
                bottleItemsToConsume = ClientSelectItemsToConsume(waterAmountNeeded,
                                                                  MaxBottlesToConsumePerRefill);
                if (bottleItemsToConsume.Count == 0)
                {
                    NotificationSystem.ClientShowNotification(
                        NotificationCannotRefill_Title,
                        NotificationCannotRefill_Message,
                        NotificationColor.Bad,
                        item.ProtoItem.Icon);
                    return null;
                }
            }

            return new WateringCanRefillRequest(character, worldObject, item, bottleItemsToConsume);
        }

        protected override void SharedOnActionCompletedInternal(
            WateringCanRefillActionState state,
            ICharacter character)
        {
            var itemWateringCan = state.ItemWateringCan;
            var protoWateringCan = (IProtoItemToolWateringCan)itemWateringCan.ProtoItem;
            var capacity = (int)protoWateringCan.WaterCapacity;
            var waterAmount = (int)protoWateringCan.SharedGetWaterAmount(itemWateringCan);
            var bottleItemsToConsumeForRefill = state.ItemsToConsumeForRefill;

            if (bottleItemsToConsumeForRefill?.Count > 0)
            {
                // try consume water from bottles
                waterAmount = SharedTryConsumeWaterBottles(state, character, waterAmount, capacity);
                if (waterAmount > capacity)
                {
                    throw new Exception("Impossible - water amount exceeded");
                }
            }
            else
            {
                // try consume water from well
                var objectWell = state.TargetWorldObject;
                // TODO: consume actual water from the well object?
                waterAmount = capacity;
            }

            protoWateringCan.SharedOnRefilled(itemWateringCan, (byte)waterAmount);
        }

        protected override WateringCanRefillActionState SharedTryCreateState(WateringCanRefillRequest request)
        {
            var itemWateringCan = request.Item;
            var worldObject = request.WorldObject;
            var character = request.Character;

            if (IsServer
                && !Server.Core.IsInPrivateScope(character, itemWateringCan))
            {
                throw new Exception(
                    $"{character} cannot access {itemWateringCan} because it's container is not in the private scope");
            }

            var waterAmountNeeded = SharedGetWaterAmountNeeded(itemWateringCan);
            if (waterAmountNeeded <= 0)
            {
                // no need to refill
                return null;
            }

            if (worldObject == null
                && request.ItemsToConsumeForRefill?.Count == 0)
            {
                throw new Exception("Cannot refill - no well object and no water bottles available");
            }

            if (IsServer && request.ItemsToConsumeForRefill != null)
            {
                foreach (var item in request.ItemsToConsumeForRefill)
                {
                    if (!Server.Core.IsInPrivateScope(character, item))
                    {
                        throw new Exception(
                            $"{character} cannot access {item} because it's container is not in the private scope");
                    }
                }
            }

            return new WateringCanRefillActionState(
                character,
                worldObject,
                durationSeconds: 1,
                itemWateringCan: itemWateringCan,
                itemsToConsumeForRefill: request.ItemsToConsumeForRefill);
        }

        protected override void SharedValidateRequest(WateringCanRefillRequest request)
        {
            var worldObject = request.WorldObject;
            if (worldObject != null)
            {
                if (!(worldObject.ProtoWorldObject is ProtoObjectWell))
                {
                    throw new Exception("The world object must be a well");
                }

                if (!worldObject.ProtoWorldObject.SharedCanInteract(request.Character, worldObject, true))
                {
                    throw new Exception("Cannot interact with " + worldObject);
                }
            }
            else if (request.ItemsToConsumeForRefill == null
                     || request.ItemsToConsumeForRefill.Count == 0)
            {
                throw new Exception("No items to consume for refill");
            }

            if (request.Item != request.Character.SharedGetPlayerSelectedHotbarItem())
            {
                throw new Exception("The item is not selected");
            }

            if (!(request.Item.ProtoItem is IProtoItemToolWateringCan))
            {
                throw new Exception("The item must be a watering can");
            }
        }

        private static List<IItem> ClientSelectItemsToConsume(int waterAmountNeeded, int maxBottlesToConsume)
        {
            var itemsToConsume = new List<IItem>();

            var character = Client.Characters.CurrentPlayerCharacter;
            var containerInventory = character.SharedGetPlayerContainerInventory();
            var containerHotbar = character.SharedGetPlayerContainerHotbar();

            var allItems = containerInventory.Items.Concat(containerHotbar.Items);
            itemsToConsume.AddRange(allItems.Where(i => i.ProtoItem is ItemBottleWater));

            for (var i = 0; i < itemsToConsume.Count; i++)
            {
                var item = itemsToConsume[i];
                waterAmountNeeded -= item.Count * BottleWaterAmount;
                maxBottlesToConsume -= item.Count;

                if (waterAmountNeeded <= 0
                    || maxBottlesToConsume <= 0)
                {
                    // that's enough - don't consume all the remaining bottles
                    itemsToConsume.RemoveRange(startingIndex: i + 1);
                }
            }

            return itemsToConsume;
        }

        private static int SharedGetWaterAmountNeeded(IItem item)
        {
            var protoWateringCan = (IProtoItemToolWateringCan)item.ProtoItem;
            var waterAmountCurrent = protoWateringCan.SharedGetWaterAmount(item);
            var waterAmountNeeded = protoWateringCan.WaterCapacity - waterAmountCurrent;
            return waterAmountNeeded;
        }

        private static int SharedTryConsumeWaterBottles(
            WateringCanRefillActionState state,
            ICharacter character,
            int waterAmount,
            int waterCapacity)
        {
            IItemsServerService serverItemsService = null;
            IItemsClientService clientItemsService = null;

            var isServer = IsServer;
            if (isServer)
            {
                serverItemsService = Server.Items;
            }
            else
            {
                clientItemsService = Client.Items;
            }

            var totalUsedBottlesCount = 0;
            var maxBottlesToConsume = MaxBottlesToConsumePerRefill;

            foreach (var itemBottle in state.ItemsToConsumeForRefill)
            {
                // check if character owns this item
                if (!character.ProtoCharacter
                              .SharedEnumerateAllContainers(character, includeEquipmentContainer: false)
                              .Any(c => c.Items.Contains(itemBottle)))
                {
                    throw new Exception("The character doesn't own " + itemBottle + " - cannot use it to reload");
                }

                int itemBottleCountToSubstract;

                var itemBottleCount = itemBottle.Count;
                if (itemBottleCount == 0)
                {
                    continue;
                }

                if (itemBottleCount > maxBottlesToConsume)
                {
                    itemBottleCount = (ushort)maxBottlesToConsume;
                }

                if (waterAmount + itemBottleCount * BottleWaterAmount
                    >= waterCapacity)
                {
                    // there are more than enough item count in that item stack to fully refill the watering can
                    itemBottleCountToSubstract =
                        (int)Math.Ceiling((waterCapacity - waterAmount) / (double)BottleWaterAmount);
                    waterAmount = waterCapacity;
                }
                else
                {
                    // consume full item stack
                    itemBottleCountToSubstract = itemBottleCount;
                    waterAmount += itemBottleCount * BottleWaterAmount;
                }

                if (itemBottleCountToSubstract > 0)
                {
                    maxBottlesToConsume -= itemBottleCountToSubstract;
                    totalUsedBottlesCount += itemBottleCountToSubstract;

                    // reduce item count
                    var itemBottleNewCount = itemBottle.Count - itemBottleCountToSubstract;
                    if (isServer)
                    {
                        serverItemsService.SetCount(
                            itemBottle,
                            itemBottleNewCount,
                            byCharacter: character,
                            // reloading is also processed on Client-side separately, so no need to send updates
                            isSendingUpdatesToPlayer: false);

                        // spawn empty bottles
                        ItemBottleEmpty.ServerSpawnEmptyBottle(character, (ushort)itemBottleCountToSubstract);
                    }
                    else // if (IsClient)
                    {
                        clientItemsService.SetCount(itemBottle, itemBottleNewCount);
                    }
                }

                if (waterAmount >= waterCapacity)
                {
                    // fully refilled
                    break;
                }

                if (maxBottlesToConsume <= 0)
                {
                    // amount of bottles to consume exceeded
                    break;
                }
            }

            if (IsClient)
            {
                NotificationSystem.ClientShowItemsNotification(
                    new Dictionary<IProtoItem, int>()
                    {
                        { GetProtoEntity<ItemBottleWater>(), -totalUsedBottlesCount }
                    });
            }

            return waterAmount;
        }
    }
}