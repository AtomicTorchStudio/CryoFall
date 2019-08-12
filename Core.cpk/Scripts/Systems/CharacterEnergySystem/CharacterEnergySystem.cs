namespace AtomicTorch.CBND.CoreMod.Systems.CharacterEnergySystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.DataStructures;

    /// <summary>
    /// Player character power system (energy from battery pack devices, don't confuse with stamina).
    /// </summary>
    public class CharacterEnergySystem : ProtoSystem<CharacterEnergySystem>
    {
        public const string NotificationCannotUse_NoPowerBanksEquipped = "No powerbanks equipped.";

        public const string NotificationCannotUse_NotEnoughCharge = "Not enough charge in powerbanks.";

        // {0} is item name
        public const string NotificationCannotUse_Title = "Cannot use {0}";

        public override string Name => "Character energy system";

        public static uint ClientCalculateTotalEnergyCapacity()
        {
            return SharedCalculateTotalEnergyCapacity(ClientCurrentCharacterHelper.Character);
        }

        public static double ClientCalculateTotalEnergyCharge()
        {
            return SharedCalculateTotalEnergyCharge(ClientCurrentCharacterHelper.Character);
        }

        public static void ClientShowNotificationNotEnoughEnergyCharge(IProtoItem itemProto)
        {
            var playNotificationSound = true;
            if (itemProto is IProtoItemWeapon protoItemWeapon)
            {
                playNotificationSound = false;
                protoItemWeapon.SoundPresetWeapon.PlaySound(WeaponSound.Empty,
                                                            volume: SoundConstants.VolumeWeapon);
            }

            var hasPowerBanks = ClientCalculateTotalEnergyCapacity() > 0;

            NotificationSystem.ClientShowNotification(
                string.Format(NotificationCannotUse_Title, itemProto.Name),
                hasPowerBanks
                    ? NotificationCannotUse_NotEnoughCharge
                    : NotificationCannotUse_NoPowerBanksEquipped,
                NotificationColor.Bad,
                itemProto.Icon,
                playSound: playNotificationSound);
        }

        public static void ServerAddEnergyCharge(ICharacter character, double energyAmountToAdd)
        {
            if (energyAmountToAdd <= 0)
            {
                return;
            }

            using var tempItemsList = SharedGetTempListPowerBanksForCharacter(character, true);
            ServerAddEnergyInternal(energyAmountToAdd, tempItemsList);
        }

        public static void ServerAddEnergyCharge(IItemsContainer container, double energyAmountToAdd)
        {
            if (energyAmountToAdd <= 0
                || container == null
                || container.IsDestroyed)
            {
                return;
            }

            ITempList<IItem> tempItemsList;
            if (container.Owner is ICharacter ownerCharacter)
            {
                tempItemsList = SharedGetTempListPowerBanksForCharacter(ownerCharacter, onlyEquippedDevices: false);
            }
            else
            {
                tempItemsList = Api.Shared.GetTempList<IItem>();
                SharedFindPowerBanks(container, tempItemsList);

                if (container.Owner is IWorldObject worldObject)
                {
                    foreach (var character in InteractionCheckerSystem.SharedEnumerateCurrentInteractionCharacters(
                        worldObject))
                    {
                        using var tempCharacterPowerBanks = SharedGetTempListPowerBanksForCharacter(character,
                                                                                                    onlyEquippedDevices:
                                                                                                    false);
                        tempItemsList.AddRange(tempCharacterPowerBanks);
                    }
                }
            }

            ServerAddEnergyInternal(energyAmountToAdd, tempItemsList);
            tempItemsList.Dispose();
        }

        /// <summary>
        /// Try to deduct energy.
        /// </summary>
        /// <returns>Returns true if there was enough energy and it was deducted.</returns>
        public static bool ServerDeductEnergyCharge(ICharacter character, double requiredEnergyAmount)
        {
            if (requiredEnergyAmount <= 0)
            {
                return true;
            }

            using var tempItemsList = SharedGetTempListPowerBanksForCharacter(character, onlyEquippedDevices: true);
            if (tempItemsList.Count == 0)
            {
                // there are no battery packs equipped
                return false;
            }

            if (!SharedHasEnergyCharge(tempItemsList, requiredEnergyAmount))
            {
                // not enough energy stored in the battery packs
                return false;
            }

            // deduct energy in reverse order
            var list = tempItemsList.AsList();
            for (var index = list.Count - 1; index >= 0; index--)
            {
                var item = list[index];
                var privateState = SharedGetPrivateState(item);
                var charge = privateState.EnergyCharge;
                if (charge <= 0)
                {
                    // no charge there
                    continue;
                }

                if (charge >= requiredEnergyAmount)
                {
                    // there are more than enough charge available
                    privateState.EnergyCharge -= requiredEnergyAmount;
                    ServerOnEnergyUsed(item, requiredEnergyAmount);
                    return true;
                }

                // use all the remaining charge in this item
                requiredEnergyAmount -= charge;
                privateState.EnergyCharge = 0;
                ServerOnEnergyUsed(item, charge);
            }

            throw new Exception("Should be impossible");
        }

        public static uint SharedCalculateTotalEnergyCapacity(ICharacter character)
        {
            using var tempItemsList = SharedGetTempListPowerBanksForCharacter(character, onlyEquippedDevices: true);
            return SharedCalculateTotalEnergyCapacity(tempItemsList);
        }

        public static double SharedCalculateTotalEnergyCharge(ICharacter character)
        {
            using var tempItemsList = SharedGetTempListPowerBanksForCharacter(character, onlyEquippedDevices: true);
            return SharedCalculateTotalEnergyCharge(tempItemsList);
        }

        public static bool SharedHasEnergyCharge(ICharacter character, double energyRequired)
        {
            using var tempItemsList = SharedGetTempListPowerBanksForCharacter(character, onlyEquippedDevices: true);
            return SharedHasEnergyCharge(tempItemsList, energyRequired);
        }

        private static void ServerAddEnergyInternal(double energyAmountToAdd, ITempList<IItem> powerBanks)
        {
            foreach (var item in powerBanks)
            {
                if (item.IsDestroyed)
                {
                    continue;
                }

                var privateState = SharedGetPrivateState(item);
                var protoItem = (IProtoItemPowerBank)item.ProtoItem;
                var capacity = protoItem.EnergyCapacity;

                var charge = privateState.EnergyCharge;
                if (charge >= capacity)
                {
                    // cannot add charge there
                    continue;
                }

                var newCharge = charge + energyAmountToAdd;
                if (newCharge <= capacity)
                {
                    // this battery can take the whole remaining energyAmountToAdd
                    privateState.EnergyCharge = newCharge;
                    return;
                }

                // add as much energy to this item as possible and go to the next item
                privateState.EnergyCharge = capacity;
                var energyAdded = capacity - charge;
                energyAmountToAdd -= energyAdded;
            }
        }

        private static void ServerOnEnergyUsed(IItem item, double energyAmountUsed)
        {
            if (energyAmountUsed <= 0)
            {
                return;
            }

            // reduce durability proportionally to the removed charge
            ItemDurabilitySystem.ServerModifyDurability(item, -(int)Math.Ceiling(energyAmountUsed));
        }

        private static uint SharedCalculateTotalEnergyCapacity(ITempList<IItem> tempItemsList)
        {
            uint result = 0;

            foreach (var item in tempItemsList)
            {
                var protoItem = ((IProtoItemPowerBank)item.ProtoItem);
                result += protoItem.EnergyCapacity;
            }

            return result;
        }

        private static double SharedCalculateTotalEnergyCharge(
            ITempList<IItem> tempItemsList,
            double stopIfEnergyExceeds = double.NaN)
        {
            var result = 0.0;
            var hasStopCondition = !double.IsNaN(stopIfEnergyExceeds);

            foreach (var item in tempItemsList)
            {
                var privateState = SharedGetPrivateState(item);
                var charge = privateState.EnergyCharge;
                result += charge;

                if (hasStopCondition
                    && result >= stopIfEnergyExceeds)
                {
                    return result;
                }
            }

            return result;
        }

        private static void SharedFindPowerBanks(IItemsContainer container, ITempList<IItem> tempList)
        {
            foreach (var item in container.Items)
            {
                if (item.ProtoItem is IProtoItemPowerBank)
                {
                    tempList.Add(item);
                }
            }
        }

        private static ItemPowerBankPrivateState SharedGetPrivateState(IItem itemPowerBank)
        {
            return itemPowerBank.GetPrivateState<ItemPowerBankPrivateState>();
        }

        private static ITempList<IItem> SharedGetTempListPowerBanksForCharacter(
            ICharacter character,
            bool onlyEquippedDevices)
        {
            var tempList = Api.Shared.GetTempList<IItem>();
            SharedFindPowerBanks(character.SharedGetPlayerContainerEquipment(), tempList);
            if (onlyEquippedDevices)
            {
                return tempList;
            }

            SharedFindPowerBanks(character.SharedGetPlayerContainerInventory(), tempList);
            SharedFindPowerBanks(character.SharedGetPlayerContainerHotbar(),    tempList);
            return tempList;
        }

        private static bool SharedHasEnergyCharge(ITempList<IItem> tempItemsList, double energyRequired)
        {
            var charge = SharedCalculateTotalEnergyCharge(tempItemsList,
                                                          stopIfEnergyExceeds: energyRequired);
            return charge >= energyRequired;
        }
    }
}