namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Drones;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Explosives;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.Items.Seeds;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemsContainerLandClaimSafeStorage : ProtoItemsContainer
    {
        private static readonly List<Type> WhitelistItemPrototypes;

        private static ItemsContainerLandClaimSafeStorage instance;

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

            if (IsClient)
            {
                return;
            }

            ServerSafeItemsSlotsCapacity = (byte)MathHelper.Clamp(
                ServerRates.Get(
                    "SafeStorageCapacity",
                    defaultValue: 24,
                    @"How many safe storage slots are allowed per base.
                          The value should be within 0-128 range.
                          Doesn't apply to PvE mode (there is no safe storage in PvE)."),
                min: 0,
                max: 128);

            if (PveSystem.ServerIsPvE)
            {
                ServerSafeItemsSlotsCapacity = 0;
            }
        }

        public ItemsContainerLandClaimSafeStorage()
        {
            instance = this;
        }

        public static event Action ClientSafeItemsSlotsCapacityChanged;

        public static byte ClientSafeItemsSlotsCapacity { get; private set; }

        public static byte ServerSafeItemsSlotsCapacity { get; }

        public static void RegisterInWhitelist<TProtoItem>()
            where TProtoItem : IProtoItem
        {
            WhitelistItemPrototypes.Add(typeof(TProtoItem));
        }

        public override bool CanAddItem(CanAddItemContext context)
        {
            if (context.ByCharacter == null
                || isCompactingNow)
            {
                return true;
            }

            // prohibit adding items to this container by any character if its capacity is exceeded
            if (context.Container.SlotsCount
                > (IsServer
                       ? ServerSafeItemsSlotsCapacity
                       : ClientSafeItemsSlotsCapacity))
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
                || container.SlotsCount <= ServerSafeItemsSlotsCapacity)
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
            if (slotsCount <= ServerSafeItemsSlotsCapacity)
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
                                                                ServerSafeItemsSlotsCapacity));
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

        private byte ServerRemote_RequestSafeItemsSlotsCapacity()
        {
            return ServerSafeItemsSlotsCapacity;
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += Refresh;

                Refresh();

                void Refresh()
                {
                    if (Api.Client.Characters.CurrentPlayerCharacter != null)
                    {
                        instance.CallServer(_ => _.ServerRemote_RequestSafeItemsSlotsCapacity())
                                .ContinueWith(t =>
                                              {
                                                  ClientSafeItemsSlotsCapacity = t.Result;
                                                  Logger.Info("Safe storage slots capacity received from server: "
                                                              + ClientSafeItemsSlotsCapacity);
                                                  Api.SafeInvoke(ClientSafeItemsSlotsCapacityChanged);
                                              },
                                              TaskContinuationOptions.ExecuteSynchronously);
                    }
                }
            }
        }
    }
}