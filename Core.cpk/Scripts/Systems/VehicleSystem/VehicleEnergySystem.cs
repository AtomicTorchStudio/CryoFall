namespace AtomicTorch.CBND.CoreMod.Systems.VehicleSystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.DataStructures;

    /// <summary>
    /// Player vehicle power system (energy from battery pack devices, don't confuse with stamina).
    /// </summary>
    public class VehicleEnergySystem : ProtoSystem<VehicleEnergySystem>
    {
        public static uint ClientCalculateCurrentVehicleTotalEnergyCharge()
        {
            var currentVehicle = ClientCurrentCharacterHelper.Character.SharedGetCurrentVehicle();
            return SharedCalculateTotalEnergyCharge(currentVehicle);
        }

        /// <summary>
        /// Try to deduct energy.
        /// </summary>
        /// <returns>Returns true if there was enough energy and it was deducted.</returns>
        public static bool ServerDeductEnergyCharge(IDynamicWorldObject vehicle, ushort requiredEnergyAmount)
        {
            return ServerDeductEnergyChargeInternal(vehicle, requiredEnergyAmount);
        }

        public static uint SharedCalculateTotalEnergyCharge(IDynamicWorldObject vehicle)
        {
            using var tempItemsList = SharedGetTempListFuelItemsForVehicle(vehicle);
            return SharedCalculateTotalEnergyCharge(tempItemsList);
        }

        public static bool SharedHasEnergyCharge(IDynamicWorldObject vehicle, uint energyRequired)
        {
            using var tempItemsList = SharedGetTempListFuelItemsForVehicle(vehicle);
            return SharedHasEnergyCharge(tempItemsList, energyRequired);
        }

        // please note that requiredEnergyAmount is <= ushort.MaxValue
        private static bool ServerDeductEnergyChargeInternal(IDynamicWorldObject vehicle, uint requiredEnergyAmount)
        {
            if (requiredEnergyAmount <= 0)
            {
                return true;
            }

            using var tempItemsList = SharedGetTempListFuelItemsForVehicle(vehicle);
            if (tempItemsList.Count == 0)
            {
                // there are no battery packs equipped
                return false;
            }

            // deduct energy
            foreach (var item in tempItemsList.AsList())
            {
                var privateState = SharedGetPrivateState(item);
                var charge = privateState.DurabilityCurrent;
                if (charge <= 0)
                {
                    // no charge there
                    continue;
                }

                if (charge >= requiredEnergyAmount)
                {
                    // there are more than enough charge available
                    ItemDurabilitySystem.ServerModifyDurability(item, -(int)requiredEnergyAmount);
                    return true;
                }

                // use all the remaining charge in this item
                requiredEnergyAmount -= charge;
                ItemDurabilitySystem.ServerModifyDurability(item, -(int)charge);
            }

            // probably consumed but not enough energy (remaining requiredEnergyAmount > 0)
            return false;
        }

        private static uint SharedCalculateTotalEnergyCharge(
            ITempList<IItem> tempItemsList,
            uint stopIfEnergyExceeds = uint.MaxValue)
        {
            ulong result = 0;

            foreach (var item in tempItemsList.AsList())
            {
                var privateState = SharedGetPrivateState(item);
                var charge = privateState.DurabilityCurrent;
                result += charge;

                if (result >= stopIfEnergyExceeds)
                {
                    break;
                }
            }

            return (uint)Math.Min(uint.MaxValue, result);
        }

        private static ItemWithDurabilityPrivateState SharedGetPrivateState(IItem itemPowerBank)
        {
            return itemPowerBank.GetPrivateState<ItemWithDurabilityPrivateState>();
        }

        private static ITempList<IItem> SharedGetTempListFuelItemsForVehicle(IDynamicWorldObject vehicle)
        {
            var result = Api.Shared.GetTempList<IItem>();
            var list = result.AsList();
            var fuelItemsContainer = vehicle.GetPrivateState<VehiclePrivateState>().FuelItemsContainer;

            // collect ordered list of fuel cell items (assume all items in the fuel container are fuel cells)
            foreach (var item in fuelItemsContainer.Items)
            {
                if (item.ProtoItem is not IProtoItemWithDurability)
                {
                    continue;
                }

                var isAdded = false;
                var itemPrivateState = item.GetPrivateState<ItemWithDurabilityPrivateState>();
                for (var index = 0; index < list.Count; index++)
                {
                    var otherItem = list[index];
                    var otherItemPrivateState = otherItem.GetPrivateState<ItemWithDurabilityPrivateState>();
                    if (itemPrivateState.DurabilityCurrent >= otherItemPrivateState.DurabilityCurrent)
                    {
                        continue;
                    }

                    isAdded = true;
                    list.Insert(index, item);
                    break;
                }

                if (!isAdded)
                {
                    list.Add(item);
                }
            }

            return result;
        }

        private static bool SharedHasEnergyCharge(ITempList<IItem> tempItemsList, uint energyRequired)
        {
            var charge = SharedCalculateTotalEnergyCharge(tempItemsList,
                                                          stopIfEnergyExceeds: energyRequired);
            return charge >= energyRequired;
        }
    }
}