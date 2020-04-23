namespace AtomicTorch.CBND.CoreMod.Systems.WateringCanRefill
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Tools.WateringCans;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
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
            }
            else
            {
                // need to consume water from bottles
                var bottleItemsToConsume = SharedFindWaterBottles(waterAmountNeeded,
                                                                  MaxBottlesToConsumePerRefill,
                                                                  Client.Characters.CurrentPlayerCharacter);
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

            return new WateringCanRefillRequest(character, worldObject, item);
        }

        protected override void SharedOnActionCompletedInternal(
            WateringCanRefillActionState state,
            ICharacter character)
        {
            if (IsClient)
            {
                return;
            }

            var worldObject = state.TargetWorldObject;
            var itemWateringCan = state.ItemWateringCan;
            var protoWateringCan = (IProtoItemToolWateringCan)itemWateringCan.ProtoItem;
            var capacity = (int)protoWateringCan.WaterCapacity;
            var waterAmount = (int)protoWateringCan.SharedGetWaterAmount(itemWateringCan);
            var waterAmountNeeded = SharedGetWaterAmountNeeded(itemWateringCan);

            List<IItem> bottlesToConsume = null;
            if (worldObject is null)
            {
                bottlesToConsume = SharedFindWaterBottles(waterAmountNeeded,
                                                          MaxBottlesToConsumePerRefill,
                                                          character);
            }

            if (worldObject is null
                && bottlesToConsume.Count == 0)
            {
                throw new Exception("Cannot refill - no well object and no water bottles available");
            }

            if (IsServer)
            {
                if (bottlesToConsume?.Count > 0)
                {
                    // try consume water from bottles
                    waterAmount = ServerTryConsumeWaterBottles(character, waterAmount, capacity, bottlesToConsume);
                    if (waterAmount > capacity)
                    {
                        throw new Exception("Impossible - water amount exceeded");
                    }
                }
                else
                {
                    // try consume water from well
                    var objectWell = worldObject;
                    // TODO: consume actual water from the well object?
                    waterAmount = capacity;
                }
            }

            protoWateringCan.SharedOnRefilled(itemWateringCan, (byte)waterAmount);
            this.CallClient(character, _ => _.ClientRemote_OnRefilled(itemWateringCan, (byte)waterAmount));
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

            List<IItem> bottlesToConsume = null;
            if (worldObject is null)
            {
                bottlesToConsume = SharedFindWaterBottles(waterAmountNeeded,
                                                          MaxBottlesToConsumePerRefill,
                                                          character);
            }

            if (worldObject is null
                && bottlesToConsume.Count == 0)
            {
                throw new Exception("Cannot refill - no well object and no water bottles available");
            }

            if (IsServer && bottlesToConsume != null)
            {
                foreach (var item in bottlesToConsume)
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
                itemWateringCan: itemWateringCan);
        }

        protected override void SharedValidateRequest(WateringCanRefillRequest request)
        {
            if (request.Item != request.Character.SharedGetPlayerSelectedHotbarItem())
            {
                throw new Exception("The item is not selected");
            }

            if (!(request.Item.ProtoItem is IProtoItemToolWateringCan))
            {
                throw new Exception("The item must be a watering can");
            }

            var worldObject = request.WorldObject;
            if (worldObject is null)
            {
                // check whether the player has any water bottles
                var waterAmountNeeded = SharedGetWaterAmountNeeded(request.Item);
                var bottleItemsToConsume = SharedFindWaterBottles(waterAmountNeeded,
                                                                  MaxBottlesToConsumePerRefill,
                                                                  request.Character);
                if (bottleItemsToConsume.Count == 0)
                {
                    throw new Exception("No items to consume for refill");
                }
            }
            else
            {
                if (!(worldObject.ProtoWorldObject is ProtoObjectWell))
                {
                    throw new Exception("The world object must be a well");
                }

                if (!worldObject.ProtoWorldObject.SharedCanInteract(request.Character, worldObject, true))
                {
                    throw new Exception("Cannot interact with " + worldObject);
                }

                // TODO: when we implement water consumption from the well, verify the well has water (for the server)
            }
        }

        private static int ServerTryConsumeWaterBottles(
            ICharacter character,
            int waterAmount,
            int waterCapacity,
            List<IItem> bottlesToConsume)
        {
            var serverItemsService = Server.Items;
            var totalUsedBottlesCount = 0;
            var maxBottlesToConsume = MaxBottlesToConsumePerRefill;

            foreach (var itemBottle in bottlesToConsume)
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
                    serverItemsService.SetCount(
                        itemBottle,
                        itemBottleNewCount,
                        byCharacter: character,
                        isSendingUpdatesToPlayer: true);

                    // spawn empty bottles
                    ItemBottleEmpty.ServerSpawnEmptyBottle(character, (ushort)itemBottleCountToSubstract);
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

            NotificationSystem.ServerSendItemsNotification(
                character,
                new Dictionary<IProtoItem, int>()
                {
                    { GetProtoEntity<ItemBottleWater>(), -totalUsedBottlesCount }
                });

            return waterAmount;
        }

        private static List<IItem> SharedFindWaterBottles(
            int waterAmountNeeded,
            int maxBottlesToConsume,
            ICharacter character)
        {
            var itemsToConsume = new List<IItem>();

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

        private void ClientRemote_OnRefilled(IItem itemWateringCan, byte waterAmount)
        {
            var protoWateringCan = (IProtoItemToolWateringCan)itemWateringCan.ProtoItem;
            protoWateringCan.SharedOnRefilled(itemWateringCan, waterAmount);
        }
    }
}