namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.DataLogs.Base;
    using AtomicTorch.CBND.CoreMod.Items.Drones;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.Items.Seeds;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public static class ClientContainerSortHelper
    {
        private static readonly Type[] TypeDrawOrder =
        {
            // internal order of equipment items
            Type<IProtoItemEquipmentFullBody>(),
            Type<IProtoItemEquipmentHead>(),
            Type<IProtoItemEquipmentArmor>(),
            Type<IProtoItemEquipmentImplant>(),
            Type<IProtoItemEquipmentDevice>(),
            // weapons
            Type<IProtoItemWeaponRanged>(),
            Type<IProtoItemWeaponMelee>(),
            // tools
            Type<IProtoItemToolWoodcutting>(),
            Type<IProtoItemToolMining>(),
            Type<IProtoItemToolToolbox>(),
            Type<IProtoItemToolCrowbar>(),
            Type<IProtoItemToolLight>(),
            Type<IProtoItemToolWateringCan>(),
            // ammo
            Type<IProtoItemAmmo>(),
            // everything else
            Type<IProtoItemMedical>(),
            Type<IProtoItemDataLog>(),
            Type<IProtoItemFuel>(),
            Type<IProtoItemLiquidStorage>(),
            Type<IProtoItemSeed>(),
            Type<IProtoItemFertilizer>(),
            Type<IProtoItemFood>()
            // and generic items come after all (no special type for that)
        };

        private static readonly SoundResource SortContainerSoundResource
            = new("UI/Container/Sort");

        // Here we specify the sort order for items.

        private static bool isInitialized;

        public static void ConsolidateItemStacks(IClientItemsContainer container)
        {
            Api.Logger.Important("Consolidating item stacks in " + container);

            using var tempAllItems = Api.Shared.WrapInTempList(container.Items);
            var allItems = tempAllItems.AsList();

            for (var index = allItems.Count - 1; index >= 0; index--)
            {
                var item = allItems[index];
                if (item.ProtoItem.IsStackable)
                {
                    container.TryStackItem(item);
                }
            }
        }

        public static void Init()
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;

            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            ClientInputContext.Start("Container sort helper")
                              .HandleButtonDown(
                                  GameButton.ContainerSort,
                                  () =>
                                  {
                                      var hitTestResult = Api.Client.UI.GetVisualInPointedPosition();
                                      if (hitTestResult is null)
                                      {
                                          return;
                                      }

                                      var itemSlotControl = VisualTreeHelperExtension.FindParentOfType(
                                                                    hitTestResult,
                                                                    typeof(ItemSlotControl)) as
                                                                ItemSlotControl;

                                      if (itemSlotControl is not null)
                                      {
                                          SortItems((IClientItemsContainer)itemSlotControl.Container);
                                          return;
                                      }

                                      var itemsContainerControl = VisualTreeHelperExtension.FindParentOfType(
                                                                          hitTestResult,
                                                                          typeof(ItemsContainerControl)) as
                                                                      ItemsContainerControl;

                                      if (itemsContainerControl is not null)
                                      {
                                          SortItems(itemsContainerControl.Container);
                                      }
                                  });
        }

        public static IOrderedEnumerable<IProtoItem> SortItemPrototypes(IEnumerable<IProtoItem> protoItems)
        {
            return protoItems.OrderBy(GetProtoItemSortIndex)
                             .ThenBy(protoItem => protoItem.Id);
        }

        private static IProtoItem GetProtoItemForSorting(IItem item)
        {
            if (item.ProtoItem is ItemDroneReservedSlot)
            {
                return ItemDroneReservedSlot.GetPublicState(item).ProtoItemDrone;
            }

            return item.ProtoItem;
        }

        private static int GetProtoItemSortIndex(IProtoItem protoItem)
        {
            var type = protoItem.GetType();
            int index = -1;

            while (++index < TypeDrawOrder.Length)
            {
                var otherType = TypeDrawOrder[index];
                if (type.IsImplementsInterface(otherType))
                {
                    return index;
                }
            }

            return int.MaxValue;
        }

        private static void SortItems(IClientItemsContainer container)
        {
            ConsolidateItemStacks(container);

            Api.Logger.Important("Sorting items in " + container);

            var sorted = container.Items
                                  .ToList()
                                  .OrderBy(item => GetProtoItemSortIndex(GetProtoItemForSorting(item)))
                                  .ThenBy(item => GetProtoItemForSorting(item).Id)
                                  .ThenByDescending(
                                      item =>
                                      {
                                          switch (item.ProtoItem)
                                          {
                                              case IProtoItemWithDurability _:
                                                  return item
                                                         .GetPrivateState<IItemWithDurabilityPrivateState>()
                                                         .DurabilityCurrent;

                                              case IProtoItemWithFreshness _:
                                                  return item
                                                         .GetPrivateState<IItemWithFreshnessPrivateState>()
                                                         .FreshnessCurrent;
                                          }

                                          if (item.ProtoItem.IsStackable)
                                          {
                                              return item.Count;
                                          }

                                          return long.MaxValue;
                                      })
                                  .ToList();

            //Api.Logger.WriteDev("Sorted:"
            //                    + Environment.NewLine
            //                    + sorted.GetJoinedString(Environment.NewLine));

            Api.Client.Items.ReorderItems(container, sorted);

            Api.Client.Audio.PlayOneShot(SortContainerSoundResource);

            TaskSortItemsContainer.Helper.ClientOnItemsContainerSorted();
        }

        private static Type Type<T>()
            where T : IProtoItem
        {
            return typeof(T);
        }
    }
}