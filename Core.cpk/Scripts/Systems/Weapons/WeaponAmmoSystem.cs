namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class WeaponAmmoSystem : ProtoSystem<WeaponAmmoSystem>
    {
        public const string NotificationNoAmmo_Message = "Cannot reload the weapon.";

        public const string NotificationNoAmmo_Title = "No ammo";

        public override string Name => "Weapon ammo system";

        public static void ClientTryAbortReloading()
        {
            var character = Api.Client.Characters.CurrentPlayerCharacter;
            if (character == null)
            {
                return;
            }

            var weaponState = PlayerCharacter.GetPrivateState(character).WeaponState;
            var weaponReloadingState = weaponState.WeaponReloadingState;
            if (weaponReloadingState == null)
            {
                return;
            }

            SharedTryAbortReloading(character, weapon: weaponReloadingState.Item);
        }

        public static void ClientTryReloadOrSwitchAmmoType(bool isSwitchAmmoType)
        {
            var character = Api.Client.Characters.CurrentPlayerCharacter;
            var currentWeaponState = PlayerCharacter.GetPrivateState(character).WeaponState;

            var itemWeapon = currentWeaponState.ActiveItemWeapon;
            if (itemWeapon == null)
            {
                // no active weapon to reload
                return;
            }

            var itemWeaponProto = (IProtoItemWeapon)itemWeapon.ProtoItem;
            if (itemWeaponProto.AmmoCapacity == 0)
            {
                // the item is non-reloadable
                return;
            }

            var itemPrivateState = itemWeapon.GetPrivateState<WeaponPrivateState>();
            var ammoCountNeed = isSwitchAmmoType
                                    ? itemWeaponProto.AmmoCapacity
                                    : (ushort)Math.Max(0, itemWeaponProto.AmmoCapacity - itemPrivateState.AmmoCount);

            if (ammoCountNeed == 0)
            {
                Logger.Info("No need to reload the weapon " + itemWeapon, character);
                return;
            }

            var compatibleAmmoGroups = SharedGetCompatibleAmmoGroups(character, itemWeaponProto);
            if (compatibleAmmoGroups.Count == 0
                && !isSwitchAmmoType)
            {
                itemWeaponProto.SoundPresetWeapon.PlaySound(WeaponSound.Empty,
                                                            character,
                                                            volume: SoundConstants.VolumeWeapon);
                NotificationSystem.ClientShowNotification(
                    NotificationNoAmmo_Title,
                    NotificationNoAmmo_Message,
                    NotificationColor.Bad,
                    itemWeaponProto.Icon,
                    playSound: false);

                if (currentWeaponState.SharedGetInputIsFiring())
                {
                    // stop using weapon item!
                    currentWeaponState.ActiveProtoWeapon.ClientItemUseFinish(itemWeapon);
                }

                return;
            }

            IProtoItemAmmo selectedProtoItemAmmo = null;

            var currentReloadingState = currentWeaponState.WeaponReloadingState;
            if (currentReloadingState == null)
            {
                // don't have reloading state - find ammo item matching current weapon ammo type
                var currentProtoItemAmmo = itemPrivateState.CurrentProtoItemAmmo;
                if (currentProtoItemAmmo == null)
                {
                    // no ammo selected in weapon - select first
                    selectedProtoItemAmmo = compatibleAmmoGroups.FirstOrDefault()?.Key;
                }
                else // if weapon already has ammo
                {
                    if (isSwitchAmmoType)
                    {
                        selectedProtoItemAmmo = SharedFindNextAmmoGroup(compatibleAmmoGroups,
                                                                        currentProtoItemAmmo)?.Key;
                        if (selectedProtoItemAmmo == currentProtoItemAmmo
                            && itemPrivateState.AmmoCount >= itemWeaponProto.AmmoCapacity)
                        {
                            // this ammo type is already loaded and it's fully reloaded
                            Logger.Info("No need to reload the weapon " + itemWeapon, character);
                            return;
                        }
                    }
                    else // simple reload requested
                    {
                        // try to find ammo of the same type as already loaded into the weapon
                        var isFound = false;
                        foreach (var ammoGroup in compatibleAmmoGroups)
                        {
                            if (ammoGroup.Key == currentProtoItemAmmo)
                            {
                                isFound = true;
                                selectedProtoItemAmmo = currentProtoItemAmmo;
                                break;
                            }
                        }

                        if (!isFound)
                        {
                            // no group selected - select first
                            selectedProtoItemAmmo = compatibleAmmoGroups.FirstOrDefault()?.Key;
                        }
                    }
                }
            }
            else
            {
                if (!isSwitchAmmoType)
                {
                    // already reloading
                    return;
                }

                // already reloading - try select another ammo type (alternate between them)
                var currentReloadingProtoItemAmmo = currentReloadingState.ProtoItemAmmo;
                selectedProtoItemAmmo = SharedFindNextAmmoGroup(compatibleAmmoGroups,
                                                                currentReloadingProtoItemAmmo)?.Key;

                if (selectedProtoItemAmmo == currentReloadingProtoItemAmmo)
                {
                    // already reloading this ammo type
                    return;
                }
            }

            if (currentReloadingState != null
                && currentReloadingState.ProtoItemAmmo == selectedProtoItemAmmo)
            {
                // already reloading with these ammo items
                return;
            }

            if (currentReloadingState == null
                && selectedProtoItemAmmo == null
                && (itemPrivateState.CurrentProtoItemAmmo == null
                    || itemPrivateState.AmmoCount == 0))
            {
                // already unloaded
                return;
            }

            // create reloading state on the Client-side
            var weaponReloadingState = new WeaponReloadingState(
                character,
                itemWeapon,
                itemWeaponProto,
                selectedProtoItemAmmo);
            currentWeaponState.WeaponReloadingState = weaponReloadingState;

            itemWeaponProto.SoundPresetWeapon.PlaySound(WeaponSound.Reload,
                                                        character,
                                                        SoundConstants.VolumeWeapon);
            Logger.Info(
                $"Weapon reloading started for {itemWeapon} reload duration: {weaponReloadingState.SecondsToReloadRemains:F2}s",
                character);

            if (weaponReloadingState.SecondsToReloadRemains <= 0)
            {
                // instant-reload weapon - perform local reloading
                SharedProcessWeaponReload(character, currentWeaponState);
            }

            // perform reload on server
            var arg = new ReloadWeaponRequest(itemWeapon, selectedProtoItemAmmo);
            Instance.CallServer(_ => _.ServerRemote_ReloadWeapon(arg));
        }

        public static void ServerTryReloadSameAmmo(ICharacter character)
        {
            var currentWeaponState = PlayerCharacter.GetPrivateState(character).WeaponState;

            var item = currentWeaponState.ActiveItemWeapon;
            if (item == null)
            {
                // no active weapon to reload
                return;
            }

            var itemProto = (IProtoItemWeapon)item.ProtoItem;
            if (itemProto.AmmoCapacity == 0)
            {
                // the item is non-reloadable
                return;
            }

            var itemPrivateState = item.GetPrivateState<WeaponPrivateState>();
            var ammoCountNeed = (ushort)Math.Max(0, itemProto.AmmoCapacity - itemPrivateState.AmmoCount);
            if (ammoCountNeed == 0)
            {
                return;
            }

            var compatibleAmmoGroups = SharedGetCompatibleAmmoGroups(character, itemProto);
            if (compatibleAmmoGroups.Count == 0)
            {
                // no ammo to reload
                return;
            }

            IProtoItemAmmo selectedProtoItemAmmo = null;

            var currentReloadingState = currentWeaponState.WeaponReloadingState;
            if (currentReloadingState != null)
            {
                // already reloading
                return;
            }

            // don't have reloading state - find ammo item matching current weapon ammo type
            var currentProtoItemAmmo = itemPrivateState.CurrentProtoItemAmmo;
            if (currentProtoItemAmmo == null)
            {
                // no ammo selected in weapon 
                return;
            }

            // simple reload requested
            // try to find ammo of the same type as already loaded into the weapon
            var isAmmoFound = false;
            foreach (var ammoGroup in compatibleAmmoGroups)
            {
                if (ammoGroup.Key == currentProtoItemAmmo)
                {
                    isAmmoFound = true;
                    selectedProtoItemAmmo = currentProtoItemAmmo;
                    break;
                }
            }

            if (!isAmmoFound)
            {
                return;
            }

            // create reloading state on the Server-side
            var weaponReloadingState = new WeaponReloadingState(
                character,
                item,
                itemProto,
                selectedProtoItemAmmo);
            currentWeaponState.WeaponReloadingState = weaponReloadingState;
            Logger.Info("Weapon started reloading without waiting for client request " + item, character);

            if (weaponReloadingState.SecondsToReloadRemains <= 0)
            {
                // instant-reload weapon - perform local reloading
                SharedProcessWeaponReload(character, currentWeaponState);
            }
        }

        public static void SharedTryAbortReloading(ICharacter character, IItem weapon)
        {
            var weaponState = PlayerCharacter.GetPrivateState(character).WeaponState;
            var weaponReloadingState = weaponState.WeaponReloadingState;
            if (weaponReloadingState == null)
            {
                return;
            }

            if (weaponReloadingState.Item != weapon)
            {
                Logger.Info("Weapon reloading cannot be aborted: weapon doesn't match - currently reloading "
                            + weaponReloadingState.Item
                            + " requested reloading for "
                            + weapon,
                            character);
                return;
            }

            weaponState.WeaponReloadingState = null;
            Logger.Info("Weapon reloading aborted: " + weaponReloadingState.Item, character);

            if (IsClient)
            {
                Instance.CallServer(_ => _.ServerRemote_AbortReloading(weaponReloadingState.Item));
            }
        }

        public static void SharedUpdateReloading(WeaponState weaponState, ICharacter character, double deltaTime)
        {
            var reloadingState = weaponState.WeaponReloadingState;
            if (reloadingState == null)
            {
                return;
            }

            // process reloading
            reloadingState.SecondsToReloadRemains -= deltaTime;
            if (reloadingState.SecondsToReloadRemains > 0)
            {
                // need more time to reload
                return;
            }

            // reloaded
            reloadingState.SecondsToReloadRemains = 0;
            SharedProcessWeaponReload(character, weaponState);

            weaponState.ShotsDone = 0;
            weaponState.ServerLastClientReportedShotsDoneCount = null;
        }

        private static IGrouping<IProtoItemAmmo, IItem> SharedFindNextAmmoGroup(
            List<IGrouping<IProtoItemAmmo, IItem>> compatibleAmmoGroups,
            IProtoItem currentProtoItemAmmo)
        {
            var foundProtoItemIndex = -1;
            for (var index = 0; index < compatibleAmmoGroups.Count; index++)
            {
                var compatibleAmmoItem = compatibleAmmoGroups[index];
                if (compatibleAmmoItem.Key == currentProtoItemAmmo)
                {
                    // found current proto item, select next item prototype
                    foundProtoItemIndex = index;
                    break;
                }
            }

            if (foundProtoItemIndex < 0)
            {
                // current proto item ammo is not found
                return compatibleAmmoGroups.FirstOrDefault();
            }

            // select next proto ammo group
            if (foundProtoItemIndex + 1 < compatibleAmmoGroups.Count)
            {
                return compatibleAmmoGroups[foundProtoItemIndex + 1];
            }

            return null;
        }

        /// <summary>
        /// Returns compatible with weapon ammo group by ammo type.
        /// </summary>
        private static List<IGrouping<IProtoItemAmmo, IItem>> SharedGetCompatibleAmmoGroups(
            ICharacter character,
            IProtoItemWeapon protoWeapon)
        {
            var compatibleAmmoProtos = protoWeapon.CompatibleAmmoProtos;
            var containerInventory = character.SharedGetPlayerContainerInventory();
            var containerHotbar = character.SharedGetPlayerContainerHotbar();

            var allItems = containerInventory.Items.Concat(containerHotbar.Items);
            return allItems
                   .Where(i => compatibleAmmoProtos.Contains(i.ProtoGameObject))
                   .GroupBy(a => (IProtoItemAmmo)a.ProtoItem)
                   .ToList();
        }

        /// <summary>
        /// Executed when a weapon must reload (after the reloading duration is completed).
        /// </summary>
        private static void SharedProcessWeaponReload(ICharacter character, WeaponState weaponState)
        {
            var weaponReloadingState = weaponState.WeaponReloadingState;

            // remove weapon reloading state
            weaponState.WeaponReloadingState = null;

            var itemWeapon = weaponReloadingState.Item;
            var itemWeaponProto = (IProtoItemWeapon)itemWeapon.ProtoGameObject;
            var itemWeaponPrivateState = itemWeapon.GetPrivateState<WeaponPrivateState>();
            var weaponAmmoCount = (int)itemWeaponPrivateState.AmmoCount;
            var weaponAmmoCapacity = itemWeaponProto.AmmoCapacity;

            var selectedProtoItemAmmo = weaponReloadingState.ProtoItemAmmo;
            var currentProtoItemAmmo = itemWeaponPrivateState.CurrentProtoItemAmmo;

            if (weaponAmmoCount > 0)
            {
                if (selectedProtoItemAmmo != currentProtoItemAmmo
                    && weaponAmmoCount > 0)
                {
                    // unload current ammo
                    if (IsServer)
                    {
                        var result = Server.Items.CreateItem(
                            toCharacter: character,
                            protoItem: currentProtoItemAmmo,
                            count: (ushort)weaponAmmoCount);

                        if (!result.IsEverythingCreated)
                        {
                            // cannot unload current ammo - no space, try to unload to the ground
                            result.Rollback();

                            var tile = Api.Server.World.GetTile(character.TilePosition);
                            var groundContainer = ObjectGroundItemsContainer
                                .ServerTryGetOrCreateGroundContainerAtTileOrNeighbors(tile);

                            if (groundContainer == null)
                            {
                                // cannot unload current ammo to the ground - no free space around character
                                Instance.CallClient(character,
                                                    _ => _.ClientRemote_NoSpaceForUnloadedAmmo(currentProtoItemAmmo));
                                return;
                            }

                            result = Server.Items.CreateItem(
                                container: groundContainer,
                                protoItem: currentProtoItemAmmo,
                                count: (ushort)weaponAmmoCount);

                            if (!result.IsEverythingCreated)
                            {
                                // cannot unload current ammo to the ground - no space in ground containers near the character
                                result.Rollback();
                                Instance.CallClient(character,
                                                    _ => _.ClientRemote_NoSpaceForUnloadedAmmo(
                                                        currentProtoItemAmmo));
                                return;
                            }

                            // notify player that there were not enough space in inventory so the items were dropped to the ground
                            NotificationSystem.ServerSendNotificationNoSpaceInInventoryItemsDroppedToGround(
                                character,
                                result.ItemAmounts.First().Key?.ProtoItem);
                        }
                    }

                    Logger.Info(
                        $"Weapon ammo unloaded: {itemWeapon} -> {weaponAmmoCount} {currentProtoItemAmmo})",
                        character);

                    weaponAmmoCount = 0;
                    itemWeaponPrivateState.AmmoCount = 0;
                }
                else // if the same ammo type is loaded              
                if (weaponAmmoCount == weaponAmmoCapacity)
                {
                    // already completely loaded
                    Logger.Info(
                        $"Weapon reloading cancelled: {itemWeapon} - no reloading is required ({weaponAmmoCount}/{weaponAmmoCapacity} {selectedProtoItemAmmo})",
                        character);
                    return;
                }
            }
            else // if ammoCount == 0
            if (selectedProtoItemAmmo == null
                && currentProtoItemAmmo == null)
            {
                Logger.Info(
                    $"Weapon reloading cancelled: {itemWeapon} - already unloaded ({weaponAmmoCount}/{weaponAmmoCapacity})",
                    character);
                return;
            }

            if (selectedProtoItemAmmo != null)
            {
                var selectedAmmoGroup = SharedGetCompatibleAmmoGroups(character, itemWeaponProto)
                    .FirstOrDefault(g => g.Key == selectedProtoItemAmmo);

                if (selectedAmmoGroup == null)
                {
                    Logger.Warning(
                        $"Weapon reloading impossible: {itemWeapon} - no ammo of the required type ({selectedProtoItemAmmo})",
                        character);
                    return;
                }

                var ammoItems = SharedSelectAmmoItemsFromGroup(selectedAmmoGroup,
                                                               ammoCountNeed: weaponAmmoCapacity - weaponAmmoCount);
                foreach (var request in ammoItems)
                {
                    var itemAmmo = request.Item;
                    if (itemAmmo.ProtoItem != selectedProtoItemAmmo)
                    {
                        Logger.Error(
                            "Trying to load multiple ammo types in one reloading: "
                            + ammoItems.Select(a => a.Item.ProtoItem).GetJoinedString(),
                            character);
                        break;
                    }

                    int ammoToSubstract;

                    var itemAmmoCount = itemAmmo.Count;
                    if (itemAmmoCount == 0)
                    {
                        continue;
                    }

                    if (request.Count != itemAmmoCount)
                    {
                        if (request.Count < itemAmmoCount)
                        {
                            itemAmmoCount = request.Count;
                        }
                        else if (IsServer)
                        {
                            Logger.Warning(
                                $"Trying to take more ammo to reload than player have: {itemAmmo} requested {request.Count}. Will reload as much as possible only.",
                                character);
                        }
                    }

                    if (weaponAmmoCount + itemAmmoCount >= weaponAmmoCapacity)
                    {
                        // there are more than enough ammo in that item stack to fully refill the weapon
                        ammoToSubstract = weaponAmmoCapacity - weaponAmmoCount;
                        weaponAmmoCount = weaponAmmoCapacity;
                    }
                    else
                    {
                        // consume full item stack
                        ammoToSubstract = itemAmmoCount;
                        weaponAmmoCount += itemAmmoCount;
                    }

                    // check if character owns this item
                    if (itemAmmo.Container.OwnerAsCharacter != character)
                    {
                        Logger.Error("The character doesn't own " + itemAmmo + " - cannot use it to reload",
                                     character);
                        continue;
                    }

                    // reduce ammo item count
                    if (IsServer)
                    {
                        Server.Items.SetCount(
                            itemAmmo,
                            itemAmmo.Count - ammoToSubstract,
                            byCharacter: character);
                    }

                    if (weaponAmmoCount == weaponAmmoCapacity)
                    {
                        // the weapon is fully reloaded, no need to subtract ammo from the next ammo items
                        break;
                    }
                }
            }

            if (currentProtoItemAmmo != selectedProtoItemAmmo)
            {
                // another ammo type selected
                itemWeaponPrivateState.CurrentProtoItemAmmo = selectedProtoItemAmmo;
                // reset weapon cache (it will be re-calculated on next fire processing)
                weaponState.WeaponCache = null;
            }

            if (weaponAmmoCount < 0
                || weaponAmmoCount > weaponAmmoCapacity)
            {
                Logger.Error(
                    "Something is completely wrong during reloading! Result ammo count is: " + weaponAmmoCount);
                weaponAmmoCount = 0;
            }

            itemWeaponPrivateState.AmmoCount = (ushort)weaponAmmoCount;

            Logger.Info(
                $"Weapon reloaded: {itemWeapon} - ammo {weaponAmmoCount}/{weaponAmmoCapacity} {selectedProtoItemAmmo?.ToString() ?? "<no ammo>"}",
                character);
        }

        private static List<ItemWithCount> SharedSelectAmmoItemsFromGroup(
            IGrouping<IProtoItemAmmo, IItem> selectedAmmoGroup,
            int ammoCountNeed)
        {
            var result = new List<ItemWithCount>();
            foreach (var item in selectedAmmoGroup)
            {
                if (ammoCountNeed == 0)
                {
                    break;
                }

                var itemCount = item.Count;
                if (itemCount <= ammoCountNeed)
                {
                    // will use part of the item to reload
                    ammoCountNeed -= itemCount;
                }
                else
                {
                    // will use the whole remaining item to reload
                    itemCount = (ushort)ammoCountNeed;
                    ammoCountNeed = 0;
                }

                result.Add(new ItemWithCount(item, itemCount));
            }

            return result;
        }

        private void ClientRemote_NoSpaceForUnloadedAmmo(IProtoItemAmmo protoAmmo)
        {
            NotificationSystem.ClientShowNotification("Cannot unload",
                                                      "No space for unloaded ammo in inventory",
                                                      NotificationColor.Bad,
                                                      protoAmmo.Icon);
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered, maxCallsPerSecond: 60)]
        private void ServerRemote_AbortReloading(IItem weapon)
        {
            var character = ServerRemoteContext.Character;
            SharedTryAbortReloading(character, weapon);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, maxCallsPerSecond: 60)]
        private void ServerRemote_ReloadWeapon(ReloadWeaponRequest args)
        {
            var character = ServerRemoteContext.Character;
            var itemWeapon = args.Item;
            if (itemWeapon == null)
            {
                throw new Exception("Item not found.");
            }

            if (!(itemWeapon.ProtoItem is IProtoItemWeapon itemProto))
            {
                throw new Exception("Not a weapon: " + itemWeapon);
            }

            if (itemProto.AmmoCapacity == 0)
            {
                throw new Exception("The weapon is not reloadable: " + itemWeapon);
            }

            if (IsServer
                && !Server.Core.IsInPrivateScope(character, itemWeapon))
            {
                throw new Exception(
                    $"{character} cannot access {itemWeapon} because it's container is not in the private scope");
            }

            if (itemWeapon.IsDestroyed
                || itemWeapon.Count < 1)
            {
                throw new Exception($"{itemWeapon} is destroyed");
            }

            var privateState = itemWeapon.GetPrivateState<WeaponPrivateState>();

            var weaponState = PlayerCharacter.GetPrivateState(character).WeaponState;
            if (weaponState == null
                || weaponState.ActiveItemWeapon != itemWeapon)
            {
                throw new Exception(
                    $"Only current active weapon could be reloaded: want to reload {itemWeapon}, but current active weapon is {weaponState?.ActiveItemWeapon}");
            }

            var selectedProtoItemAmmo = args.ProtoItemAmmo;
            var ammoCurrent = privateState.AmmoCount;
            var ammoMax = itemProto.AmmoCapacity;

            if (weaponState.WeaponReloadingState == null
                && ammoCurrent == ammoMax
                && privateState.CurrentProtoItemAmmo == selectedProtoItemAmmo)
            {
                Logger.Warning("Weapon is already full, no need to reload " + itemWeapon, character);
                return;
            }

            if (weaponState.WeaponReloadingState != null
                && weaponState.WeaponReloadingState.ProtoItemAmmo == selectedProtoItemAmmo)
            {
                Logger.Info("Weapon is already reloading this ammo, no need to reload " + itemWeapon, character);
                return;
            }

            // create reloading state on the Server-side
            var weaponReloadingState = new WeaponReloadingState(
                character,
                itemWeapon,
                itemProto,
                selectedProtoItemAmmo);
            weaponState.WeaponReloadingState = weaponReloadingState;

            Logger.Info(
                $"Weapon reloading started for {itemWeapon} reload duration: {weaponReloadingState.SecondsToReloadRemains:F2}s",
                character);

            if (weaponReloadingState.SecondsToReloadRemains == 0)
            {
                // instant-reloading weapon
                SharedProcessWeaponReload(character, weaponState);
            }
        }

        private readonly struct ItemWithCount
        {
            public ItemWithCount(IItem item, ushort count)
            {
                this.Item = item;
                this.Count = count;
            }

            public ushort Count { get; }

            public IItem Item { get; }
        }
    }
}