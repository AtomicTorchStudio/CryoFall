namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Drones;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Explosives;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.Items.Seeds;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ItemsContainerLandClaimSafeStorage : ProtoItemsContainer
    {
        private static readonly List<Type> WhitelistItemPrototypes;

        private static bool isCompactingNow;

        static ItemsContainerLandClaimSafeStorage()
        {
            WhitelistItemPrototypes = new List<Type>()
            {
                typeof(IProtoItemAmmo),
                typeof(IProtoItemEquipment), // including devices and implants
                typeof(IProtoItemExplosive),
                typeof(IProtoItemMedical),
                typeof(IProtoItemSeed),
                typeof(IProtoItemTool),
                typeof(IProtoItemFuelElectricity),
                typeof(IProtoItemWeapon),
                typeof(IProtoItemDrone),
                typeof(IProtoItemDroneControl)
            };
        }

        public override bool CanAddItem(CanAddItemContext context)
        {
            if (context.ByCharacter is null
                || isCompactingNow)
            {
                return true;
            }

            // prohibit adding items to this container by any character if its capacity is exceeded
            if (context.Container.SlotsCount
                > RatePvPSafeStorageCapacity.SharedValue)
            {
                return false;
            }

            if (!SharedIsAllowedItemProto(context.Item.ProtoItem))
            {
                return false;
            }

            return true;
        }

        public override void ServerOnItemRemoved(IItemsContainer container, IItem item, ICharacter character)
        {
            base.ServerOnItemRemoved(container, item, character);

            if (isCompactingNow
                || container.SlotsCount <= RatePvPSafeStorageCapacity.SharedValue)
            {
                return;
            }

            // Invoke the container compacting a bit later.
            // Required when player is invoking a batch items move
            // (so we wait until the batched operations are completed).
            ServerTimersSystem.AddAction(
                delaySeconds: 0.05,
                () => ServerTryCompactContainer(container));
        }

        private static void ServerTryCompactContainer(IItemsContainer container)
        {
            if (isCompactingNow
                || container.IsDestroyed)
            {
                return;
            }

            var slotsCount = container.SlotsCount;
            if (slotsCount <= RatePvPSafeStorageCapacity.SharedValue)
            {
                return;
            }

            try
            {
                isCompactingNow = true;
                using var tempList = Api.Shared.WrapInTempList(container.Items);
                Server.Items.ReorderItems(container, tempList.AsList());
                Server.Items.SetSlotsCount(container,
                                           slotsCount: Math.Max(container.OccupiedSlotsCount,
                                                                RatePvPSafeStorageCapacity.SharedValue));
            }
            finally
            {
                isCompactingNow = false;
            }
        }

        private static bool SharedIsAllowedItemProto(IProtoItem protoItem)
        {
            // check in the whitelist
            var itemType = protoItem.GetType();
            foreach (var type in WhitelistItemPrototypes)
            {
                if (type.IsAssignableFrom(itemType))
                {
                    return true;
                }
            }

            return false;
        }
    }
}