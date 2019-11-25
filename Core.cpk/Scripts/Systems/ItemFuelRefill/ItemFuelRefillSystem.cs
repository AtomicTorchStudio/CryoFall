namespace AtomicTorch.CBND.CoreMod.Systems.ItemFuelRefill
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ItemFuelRefillSystem
        : ProtoActionSystem<
            ItemFuelRefillSystem,
            ItemFuelRefillRequest,
            ItemFuelRefillActionState,
            ItemFuelRefillActionState.PublicState>
    {
        public const string NotificationNeedFuel_MessageFormat
            = @"To refill you need to have a fuel item:
  [br]{0}.";

        public const string NotificationNeedFuel_Title = "Cannot refill";

        public const string NotificationNoNeedToRefill = "No need to refill";

        public const string NotificationNotRefillable = "This item is not refillable.";

        private const int MaxItemsToConsumePerRefill = 1;

        public override string Name => "Item fuel refill system";

        public static void ServerNotifyItemRefilled(IItem item)
        {
            using var scopedBy = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(item, scopedBy);
            if (scopedBy.Count == 0)
            {
                return;
            }

            var currentFuelAmount = item.GetPrivateState<ItemWithFuelPrivateState>()
                                        .FuelAmount;
            Instance.CallClient(scopedBy, _ => _.ClientRemote_SetItemFuelAmount(item, currentFuelAmount));
        }

        public ItemFuelRefillRequest ClientTryCreateRequest(ICharacter character, IItem item)
        {
            if (!(item?.ProtoGameObject is IProtoItemWithFuel protoItemFuelRefillable))
            {
                // no item selected
                return null;
            }

            if (protoItemFuelRefillable.ItemFuelConfig.FuelProtoItemsList.Count == 0)
            {
                if (IsServer)
                {
                    Logger.Warning("Cannot refill - not refillable: " + item, character);
                }
                else // if (IsClient)
                {
                    NotificationSystem.ClientShowNotification(
                        NotificationNotRefillable,
                        color: NotificationColor.Bad,
                        icon: protoItemFuelRefillable.Icon);
                }

                return null;
            }

            if (!SharedIsRefuelNeeded(item))
            {
                if (IsClient)
                {
                    NotificationSystem.ClientShowNotification(
                        NotificationNoNeedToRefill,
                        color: NotificationColor.Neutral,
                        icon: protoItemFuelRefillable.Icon);
                }

                return null;
            }

            if (!protoItemFuelRefillable.ClientCanStartRefill(item))
            {
                return null;
            }

            var fuelItemsToConsume = ClientSelectItemsToConsume(protoItemFuelRefillable,
                                                                amountNeeded: SharedGetFuelAmountNeeded(item),
                                                                MaxItemsToConsumePerRefill);
            if (fuelItemsToConsume.Count > 0)
            {
                return new ItemFuelRefillRequest(character, item, fuelItemsToConsume);
            }

            NotificationSystem.ClientShowNotification(
                NotificationNeedFuel_Title,
                message: string.Format(NotificationNeedFuel_MessageFormat,
                                       protoItemFuelRefillable.ItemFuelConfig.FuelProtoItemsList.Select(i => i.Name)
                                                              .GetJoinedString(", ")),
                color: NotificationColor.Bad,
                icon: protoItemFuelRefillable.Icon);
            return null;
        }

        protected override ItemFuelRefillRequest ClientTryCreateRequest(ICharacter character)
        {
            var item = character.SharedGetPlayerSelectedHotbarItem();
            return this.ClientTryCreateRequest(character, item);
        }

        protected override void SharedOnActionCompletedInternal(ItemFuelRefillActionState state, ICharacter character)
        {
            if (IsClient)
            {
                return;
            }

            var item = state.Item;
            var protoItemFuelRefillable = (IProtoItemWithFuel)item.ProtoItem;
            var capacity = protoItemFuelRefillable.ItemFuelConfig.FuelCapacity;

            var currentFuelAmount = protoItemFuelRefillable.ItemFuelConfig
                                                           .SharedGetFuelAmount(item);

            currentFuelAmount = ServerTryConsumeFuelItems(state,
                                                          character,
                                                          currentFuelAmount,
                                                          capacity);
            if (currentFuelAmount > capacity)
            {
                throw new Exception("Impossible - amount exceeded");
            }

            protoItemFuelRefillable.ItemFuelConfig
                                   .SharedOnRefilled(item,
                                                     currentFuelAmount,
                                                     serverNotifyClients: true);
        }

        protected override ItemFuelRefillActionState SharedTryCreateState(ItemFuelRefillRequest request)
        {
            var character = request.Character;
            var item = request.Item;
            var protoItemWithFuel = (IProtoItemWithFuel)item.ProtoItem;

            if (!SharedIsRefuelNeeded(item))
            {
                // no refill required
                if (IsServer)
                {
                    // update client fuel amount for the item
                    var currentFuelAmount = protoItemWithFuel.ItemFuelConfig.SharedGetFuelAmount(item);
                    this.CallClient(character, _ => _.ClientRemote_SetItemFuelAmount(item, currentFuelAmount));
                }

                return null;
            }

            ////// Commented out: the network communication between the client and the server is not instant
            ////// and client might request refilling together with the item turning off.
            //// cannot refill while active
            //if (IsServer)
            //{
            //    Logger.Warning("Cannot refill while active: " + item, character);
            //    return null;
            //}

            if (request.ItemsToConsumeForRefill?.Count == 0)
            {
                throw new Exception("Cannot refill - no well object and no water bottles available");
            }

            var refillDuration = protoItemWithFuel.ItemFuelConfig.RefillDuration;
            if (IsClient)
            {
                // add ping duration - because the refill happens only on the Server-side
                refillDuration += Client.CurrentGame.PingGameSeconds;
            }

            return new ItemFuelRefillActionState(
                character,
                durationSeconds: refillDuration,
                item: item,
                itemsToConsumeForRefill: request.ItemsToConsumeForRefill);
        }

        protected override void SharedValidateRequest(ItemFuelRefillRequest request)
        {
            if (request.ItemsToConsumeForRefill.Count == 0)
            {
                throw new Exception("No items to consume for refill");
            }

            if (request.Item.Container.OwnerAsCharacter != request.Character)
            {
                throw new Exception("The item is not owned");
            }

            //if (request.Item != request.Character.SharedGetPlayerSelectedHotbarItem())
            //{
            //    throw new Exception("The item is not selected");
            //}

            if (!(request.Item.ProtoItem is IProtoItemWithFuel))
            {
                throw new Exception("The item must be refillable");
            }
        }

        private static List<IItem> ClientSelectItemsToConsume(
            IProtoItemWithFuel protoItemWithFuel,
            double amountNeeded,
            int maxItemsToConsume)
        {
            var fuelProtoItems = protoItemWithFuel.ItemFuelConfig.FuelProtoItemsList;
            var itemsToConsume = new List<IItem>();

            var character = Client.Characters.CurrentPlayerCharacter;
            var containerInventory = character.SharedGetPlayerContainerInventory();
            var containerHotbar = character.SharedGetPlayerContainerHotbar();

            var allItems = containerInventory.Items.Concat(containerHotbar.Items);
            itemsToConsume.AddRange(allItems.Where(i => fuelProtoItems.Contains(i.ProtoItem)));
            itemsToConsume.SortBy(i => ((IProtoItemFuel)i.ProtoItem).FuelAmount);

            for (var i = 0; i < itemsToConsume.Count; i++)
            {
                var item = itemsToConsume[i];
                amountNeeded -= item.Count * ((IProtoItemFuel)item.ProtoItem).FuelAmount;
                maxItemsToConsume -= item.Count;

                if (amountNeeded <= 0
                    || maxItemsToConsume <= 0)
                {
                    // that's enough - don't consume all the remaining items
                    itemsToConsume.RemoveRange(startingIndex: i + 1);
                }
            }

            return itemsToConsume;
        }

        private static double ServerTryConsumeFuelItems(
            ItemFuelRefillActionState state,
            ICharacter character,
            double fuelAmount,
            double fuelCapacity)
        {
            var maxItemsToConsumePerRefill = MaxItemsToConsumePerRefill;
            var consumedItems = new Dictionary<IProtoItem, int>();

            foreach (var itemToConsume in state.ItemsToConsumeForRefill)
            {
                // check if character owns this item
                if (!character.ProtoCharacter
                              .SharedEnumerateAllContainers(character, includeEquipmentContainer: false)
                              .Any(c => c.Items.Contains(itemToConsume)))
                {
                    throw new Exception("The character doesn't own " + itemToConsume + " - cannot use it to reload");
                }

                int itemToConsumeCountToSubstract;

                var itemToConsumeCount = itemToConsume.Count;
                if (itemToConsumeCount == 0)
                {
                    continue;
                }

                if (itemToConsumeCount > maxItemsToConsumePerRefill)
                {
                    itemToConsumeCount = (ushort)maxItemsToConsumePerRefill;
                }

                var fuelAmountPerItem = ((IProtoItemFuel)itemToConsume.ProtoItem).FuelAmount;
                if (fuelAmount + itemToConsumeCount * fuelAmountPerItem
                    >= fuelCapacity)
                {
                    // there are more than enough item count in that item stack to fully refill
                    itemToConsumeCountToSubstract = (int)Math.Ceiling((fuelCapacity - fuelAmount)
                                                                      / fuelAmountPerItem);
                    fuelAmount = fuelCapacity;
                }
                else
                {
                    // consume full item stack
                    itemToConsumeCountToSubstract = itemToConsumeCount;
                    fuelAmount += itemToConsumeCount * fuelAmountPerItem;
                }

                if (itemToConsumeCountToSubstract > 0)
                {
                    maxItemsToConsumePerRefill -= itemToConsumeCountToSubstract;

                    // reduce item count
                    var itemToConsumeNewCount = itemToConsume.Count - itemToConsumeCountToSubstract;
                    Server.Items.SetCount(
                        itemToConsume,
                        itemToConsumeNewCount,
                        byCharacter: character);

                    // ReSharper disable once PossibleNullReferenceException
                    consumedItems.Add(itemToConsume.ProtoItem, -itemToConsumeCountToSubstract);
                }

                if (fuelAmount >= fuelCapacity)
                {
                    // fully refilled
                    break;
                }

                if (maxItemsToConsumePerRefill <= 0)
                {
                    // amount of bottles to consume exceeded
                    break;
                }
            }

            NotificationSystem.ServerSendItemsNotification(character, consumedItems);
            return fuelAmount;
        }

        private static double SharedGetFuelAmountNeeded(IItem item)
        {
            var protoItem = (IProtoItemWithFuel)item.ProtoItem;
            var currentFuelAmount = protoItem.ItemFuelConfig.SharedGetFuelAmount(item);
            var fuelAmountNeeded = protoItem.ItemFuelConfig.FuelCapacity - currentFuelAmount;
            return fuelAmountNeeded;
        }

        private static bool SharedIsRefuelNeeded(IItem item)
        {
            var protoItemWithFuel = (IProtoItemWithFuel)item.ProtoItem;
            var fuelAmountCurrent = protoItemWithFuel.ItemFuelConfig.SharedGetFuelAmount(item);

            // refuel is needed only if fuel below 99% percents
            return fuelAmountCurrent / protoItemWithFuel.ItemFuelConfig.FuelCapacity
                   < 0.99;
        }

        private void ClientRemote_SetItemFuelAmount(IItem item, double fuelAmount)
        {
            var protoItemFuelRefillable = (IProtoItemWithFuel)item.ProtoItem;
            protoItemFuelRefillable.ItemFuelConfig
                                   .SharedOnRefilled(item,
                                                     fuelAmount,
                                                     serverNotifyClients: false);

            protoItemFuelRefillable.ClientOnRefilled(
                item,
                isCurrentHotbarItem: item == ClientCurrentCharacterHelper.PublicState.SelectedItem);
        }
    }
}