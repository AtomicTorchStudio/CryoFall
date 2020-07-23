namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class WeaponAmmoSystem : ProtoSystem<WeaponAmmoSystem>
    {
        public const string NotificationNoAmmo_Message = "Cannot reload the weapon.";

        public const string NotificationNoAmmo_Title = "No ammo";

        public override string Name => "Weapon ammo system";

        public static void ClientTryAbortReloading()
        {
            var character = Api.Client.Characters.CurrentPlayerCharacter;
            if (character is null)
            {
                return;
            }

            var weaponState = PlayerCharacter.GetPrivateState(character).WeaponState;
            var weaponReloadingState = weaponState.WeaponReloadingState;
            if (weaponReloadingState is null)
            {
                return;
            }

            SharedTryAbortReloading(character, weapon: weaponReloadingState.Item);
        }

        public static void ClientTryReloadOrSwitchAmmoType(
            bool isSwitchAmmoType,
            bool sendToServer = true,
            bool? showNotificationIfNoAmmo = null)
        {
            var character = Api.Client.Characters.CurrentPlayerCharacter;
            var currentWeaponState = PlayerCharacter.GetPrivateState(character).WeaponState;

            var itemWeapon = currentWeaponState.ItemWeapon;
            if (itemWeapon is null)
            {
                // no active weapon to reload
                return;
            }

            var protoWeapon = (IProtoItemWeapon)itemWeapon.ProtoItem;
            if (protoWeapon.AmmoCapacity == 0)
            {
                // the item is non-reloadable
                return;
            }

            var itemPrivateState = itemWeapon.GetPrivateState<WeaponPrivateState>();
            var ammoCountNeed = isSwitchAmmoType
                                    ? protoWeapon.AmmoCapacity
                                    : (ushort)Math.Max(0, protoWeapon.AmmoCapacity - itemPrivateState.AmmoCount);

            if (ammoCountNeed == 0)
            {
                Logger.Info("No need to reload the weapon " + itemWeapon, character);
                return;
            }

            var compatibleAmmoGroups = SharedGetCompatibleAmmoGroups(character, protoWeapon);
            if (compatibleAmmoGroups.Count == 0
                && !isSwitchAmmoType)
            {
                if (showNotificationIfNoAmmo.HasValue && showNotificationIfNoAmmo.Value
                    || currentWeaponState.SharedGetInputIsFiring())
                {
                    protoWeapon.SoundPresetWeapon.PlaySound(WeaponSound.Empty,
                                                            character,
                                                            volume: SoundConstants.VolumeWeapon);
                    NotificationSystem.ClientShowNotification(
                        NotificationNoAmmo_Title,
                        NotificationNoAmmo_Message,
                        NotificationColor.Bad,
                        protoWeapon.Icon,
                        playSound: false);
                }

                if (currentWeaponState.SharedGetInputIsFiring())
                {
                    // stop firing the weapon
                    currentWeaponState.ProtoWeapon.ClientItemUseFinish(itemWeapon);
                }

                return;
            }

            IProtoItemAmmo selectedProtoItemAmmo = null;

            var currentReloadingState = currentWeaponState.WeaponReloadingState;
            if (currentReloadingState is null)
            {
                // don't have reloading state - find ammo item matching current weapon ammo type
                var currentProtoItemAmmo = itemPrivateState.CurrentProtoItemAmmo;
                if (currentProtoItemAmmo is null)
                {
                    // no ammo selected in weapon
                    selectedProtoItemAmmo = SharedFindNextAmmoGroup(protoWeapon.CompatibleAmmoProtos,
                                                                    compatibleAmmoGroups,
                                                                    currentProtoItemAmmo: null)?.Key;
                }
                else // if weapon already has ammo
                {
                    if (isSwitchAmmoType)
                    {
                        selectedProtoItemAmmo = SharedFindNextAmmoGroup(protoWeapon.CompatibleAmmoProtos,
                                                                        compatibleAmmoGroups,
                                                                        currentProtoItemAmmo)?.Key;
                        if (selectedProtoItemAmmo == currentProtoItemAmmo
                            && itemPrivateState.AmmoCount >= protoWeapon.AmmoCapacity)
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
                            isSwitchAmmoType = true;
                            sendToServer = true;
                            selectedProtoItemAmmo = SharedFindNextAmmoGroup(protoWeapon.CompatibleAmmoProtos,
                                                                            compatibleAmmoGroups,
                                                                            currentProtoItemAmmo: null)?.Key;
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
                selectedProtoItemAmmo = SharedFindNextAmmoGroup(protoWeapon.CompatibleAmmoProtos,
                                                                compatibleAmmoGroups,
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

            if (currentReloadingState is null
                && selectedProtoItemAmmo is null
                && itemPrivateState.CurrentProtoItemAmmo is null)
            {
                // already unloaded
                return;
            }

            // create reloading state on the Client-side
            var weaponReloadingState = new WeaponReloadingState(
                character,
                itemWeapon,
                protoWeapon,
                selectedProtoItemAmmo);
            currentWeaponState.WeaponReloadingState = weaponReloadingState;

            protoWeapon.SoundPresetWeapon.PlaySound(WeaponSound.Reload,
                                                    character,
                                                    SoundConstants.VolumeWeapon);
            Logger.Info(
                $"Weapon reloading started for {itemWeapon} reload duration: {weaponReloadingState.SecondsToReloadRemains:F2}s",
                character);

            if (weaponReloadingState.SecondsToReloadRemains <= 0)
            {
                // instant-reload weapon - perform local reloading
                SharedProcessWeaponReload(character, currentWeaponState, out _);
            }

            if (sendToServer || isSwitchAmmoType)
            {
                // perform reload on server
                var arg = new ReloadWeaponRequest(itemWeapon, selectedProtoItemAmmo);
                Instance.CallServer(_ => _.ServerRemote_ReloadWeapon(arg));
            }
        }

        /// <summary>
        /// Some weapons, such as flintlock pistol or a musket/double-barreled shotgun
        /// might have a desync issue when they're reloaded on the server
        /// right after receiving a command to stop firing.
        /// So the shots done on the client will be not done on the server.
        /// To prevent this issue, this method detects such weapons and keeps the shots flow.
        /// </summary>
        public static bool IsResetsShotsDoneNumberOnReload(IProtoItemWeapon protoWeapon)
        {
            if (protoWeapon.AmmoCapacity == 0
                || protoWeapon.AmmoConsumptionPerShot == 0)
            {
                return true;
            }

            var shotsPerMagazine = protoWeapon.AmmoCapacity / protoWeapon.AmmoConsumptionPerShot;
            if (shotsPerMagazine <= 1
                || shotsPerMagazine <= 2 && protoWeapon.FireInterval < 0.5)
            {
                // don't reset the shots done number for this weapon
                //Logger.Dev("Don't reset - shotsPerMagazine: " + shotsPerMagazine + " - " + protoWeapon.ShortId);
                return false;
            }

            //Logger.Dev("Reset - shotsPerMagazine: " + shotsPerMagazine + " - " + protoWeapon.ShortId);
            return true;
        }

        public static void ServerTryReloadSameAmmo(ICharacter character)
        {
            var weaponState = PlayerCharacter.GetPrivateState(character).WeaponState;

            var item = weaponState.ItemWeapon;
            if (item is null)
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

            var currentReloadingState = weaponState.WeaponReloadingState;
            if (currentReloadingState != null)
            {
                // already reloading
                return;
            }

            // don't have reloading state - find ammo item matching current weapon ammo type
            var currentProtoItemAmmo = itemPrivateState.CurrentProtoItemAmmo;
            if (currentProtoItemAmmo is null)
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
            weaponState.WeaponReloadingState = weaponReloadingState;
            //Logger.Dev("Weapon started reloading without a client request " + item, character);

            if (weaponReloadingState.SecondsToReloadRemains <= 0)
            {
                // instant-reload weapon - perform local reloading
                SharedProcessWeaponReload(character, weaponState, out _);
            }
            else if (IsServer)
            {
                ServerNotifyAboutReloading(character, weaponState, isFinished: false);
            }
        }

        public static int SharedGetTotalAvailableAmmo(IProtoItemAmmo protoItemAmmo, ICharacter character)
        {
            int result = 0;
            foreach (var container in SharedGetTargetContainersForCharacterAmmo(character,
                                                                                isForAmmoUnloading: false))
            {
                result += container.CountItemsOfType(protoItemAmmo);
            }

            return result;
        }

        public static void SharedTryAbortReloading(ICharacter character, IItem weapon)
        {
            var weaponState = PlayerCharacter.GetPrivateState(character).WeaponState;
            var weaponReloadingState = weaponState.WeaponReloadingState;
            if (weaponReloadingState is null)
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
            if (reloadingState is null)
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
            SharedProcessWeaponReload(character,
                                      weaponState,
                                      out var isAmmoTypeChanged);

            if (isAmmoTypeChanged
                || IsResetsShotsDoneNumberOnReload(weaponState.ProtoWeapon))
            {
                weaponState.ShotsDone = 0;
                weaponState.ServerLastClientReportedShotsDoneCount = null;
                weaponState.CustomTargetPosition = null;
                //Api.Logger.Dev("Reset ServerLastClientReportedShotsDoneCount. Last value: "
                //               + weaponState.ServerLastClientReportedShotsDoneCount);
            }

            weaponState.FirePatternCooldownSecondsRemains = 0;
            weaponState.IsIdleAutoReloadingAllowed = true;
        }

        // send notification about reloading to players in scope (so they can play a sound)
        private static void ServerNotifyAboutReloading(ICharacter character, WeaponState weaponState, bool isFinished)
        {
            using var scopedBy = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(character, scopedBy);
            scopedBy.Remove(character);
            if (scopedBy.Count == 0)
            {
                return;
            }

            if (isFinished)
            {
                Instance.CallClient(scopedBy.AsList(),
                                    _ => _.ClientRemote_OnOtherCharacterReloaded(character, weaponState.ProtoWeapon));
            }
            else
            {
                Instance.CallClient(scopedBy.AsList(),
                                    _ => _.ClientRemote_OnOtherCharacterReloading(character, weaponState.ProtoWeapon));
            }
        }

        private static IGrouping<IProtoItemAmmo, IItem> SharedFindNextAmmoGroup(
            IReadOnlyList<IProtoItemAmmo> protoWeaponCompatibleAmmoProtos,
            List<IGrouping<IProtoItemAmmo, IItem>> existingCompatibleAmmoGroups,
            IProtoItemAmmo currentProtoItemAmmo)
        {
            var ammoIndex = -1;
            for (var index = 0; index < protoWeaponCompatibleAmmoProtos.Count; index++)
            {
                var compatibleAmmoItem = protoWeaponCompatibleAmmoProtos[index];
                if (compatibleAmmoItem == currentProtoItemAmmo)
                {
                    // found current proto item, select next item prototype
                    ammoIndex = index;
                    break;
                }
            }

            if (ammoIndex < 0)
            {
                ammoIndex = -1;
            }

            // try to find next available ammo
            do
            {
                ammoIndex++;
                if (ammoIndex >= protoWeaponCompatibleAmmoProtos.Count)
                {
                    // unload weapon
                    return null;
                }

                var requiredAmmoType = protoWeaponCompatibleAmmoProtos[ammoIndex];

                foreach (var availableAmmo in existingCompatibleAmmoGroups)
                {
                    if (availableAmmo.Key == requiredAmmoType)
                    {
                        // found required ammo
                        return availableAmmo;
                    }
                }
            }
            while (true);
        }

        /// <summary>
        /// Returns compatible with weapon ammo group by ammo type.
        /// Please note that it works only for player characters.
        /// </summary>
        private static List<IGrouping<IProtoItemAmmo, IItem>> SharedGetCompatibleAmmoGroups(
            ICharacter character,
            IProtoItemWeapon protoWeapon)
        {
            var compatibleAmmoProtos = protoWeapon.CompatibleAmmoProtos;

            using var allItems = Api.Shared.GetTempList<IItem>();
            foreach (var container in SharedGetTargetContainersForCharacterAmmo(character, isForAmmoUnloading: false))
            {
                foreach (var item in container.Items)
                {
                    if (compatibleAmmoProtos.Contains(item.ProtoItem))
                    {
                        allItems.Add(item);
                    }
                }
            }

            return allItems
                   .AsList()
                   .GroupBy(a => (IProtoItemAmmo)a.ProtoItem)
                   .ToList();
        }

        private static IEnumerable<IItemsContainer> SharedGetTargetContainersForCharacterAmmo(
            ICharacter character,
            bool isForAmmoUnloading)
        {
            var currentVehicle = character.SharedGetCurrentVehicle();
            if (currentVehicle is null
                || currentVehicle.IsDestroyed
                || !currentVehicle.IsInitialized
                || ((IProtoVehicle)currentVehicle.ProtoGameObject).IsPlayersHotbarAndEquipmentItemsAllowed)
            {
                // no vehicle — use only character's containers
                return character.ProtoCharacter.SharedEnumerateAllContainers(
                    character,
                    includeEquipmentContainer: false);
            }

            // select items from the vehicle equipment container
            var targetContainers = new List<IItemsContainer>();
            targetContainers.Add(currentVehicle
                                 .GetPrivateState<VehicleMechPrivateState>()
                                 .EquipmentItemsContainer);

            if (isForAmmoUnloading)
            {
                // append character's containers
                targetContainers.AddRange(character.ProtoCharacter.SharedEnumerateAllContainers(
                                              character,
                                              includeEquipmentContainer: false));
            }

            return targetContainers;
        }

        /// <summary>
        /// Executed when a weapon must reload (after the reloading duration is completed).
        /// </summary>
        private static void SharedProcessWeaponReload(
            ICharacter character,
            WeaponState weaponState,
            out bool isAmmoTypeChanged)
        {
            var weaponReloadingState = weaponState.WeaponReloadingState;

            // remove weapon reloading state
            weaponState.WeaponReloadingState = null;

            var itemWeapon = weaponReloadingState.Item;
            var itemWeaponProto = (IProtoItemWeapon)itemWeapon.ProtoGameObject;
            var itemWeaponPrivateState = itemWeapon.GetPrivateState<WeaponPrivateState>();
            var weaponAmmoCount = (int)itemWeaponPrivateState.AmmoCount;
            var weaponAmmoCapacity = itemWeaponProto.AmmoCapacity;
            isAmmoTypeChanged = false;

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
                        var targetContainers =
                            SharedGetTargetContainersForCharacterAmmo(character, isForAmmoUnloading: true);
                        var result = Server.Items.CreateItem(
                            protoItem: currentProtoItemAmmo,
                            new AggregatedItemsContainers(targetContainers),
                            count: (ushort)weaponAmmoCount);

                        if (!result.IsEverythingCreated)
                        {
                            // cannot unload current ammo - no space, try to unload to the ground
                            result.Rollback();

                            var tile = Api.Server.World.GetTile(character.TilePosition);
                            var groundContainer = ObjectGroundItemsContainer
                                .ServerTryGetOrCreateGroundContainerAtTileOrNeighbors(character, tile);

                            if (groundContainer is null)
                            {
                                // cannot unload current ammo to the ground - no free space around character
                                NotificationSystem.ServerSendNotificationNoSpaceInInventory(character);
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
                                NotificationSystem.ServerSendNotificationNoSpaceInInventory(character);
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
                    itemWeaponPrivateState.SetAmmoCount(0);
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
            if (selectedProtoItemAmmo is null
                && currentProtoItemAmmo is null)
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

                if (selectedAmmoGroup is null)
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
                    Api.Assert(itemAmmo.ProtoItem == selectedProtoItemAmmo, "Sanity check");

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
                isAmmoTypeChanged = true;
            }

            if (weaponAmmoCount < 0
                || weaponAmmoCount > weaponAmmoCapacity)
            {
                Logger.Error(
                    "Something is completely wrong during reloading! Result ammo count is: " + weaponAmmoCount);
                weaponAmmoCount = 0;
            }

            itemWeaponPrivateState.SetAmmoCount((ushort)weaponAmmoCount);

            if (weaponAmmoCount == 0)
            {
                // weapon unloaded - and the log entry about this is already written (see above)
                return;
            }

            Logger.Info(
                $"Weapon reloaded: {itemWeapon} - ammo {weaponAmmoCount}/{weaponAmmoCapacity} {selectedProtoItemAmmo?.ToString() ?? "<no ammo>"}",
                character);

            if (IsServer)
            {
                ServerNotifyAboutReloading(character, weaponState, isFinished: true);
            }
            else
            {
                weaponState.ProtoWeapon.SoundPresetWeapon
                           .PlaySound(WeaponSound.ReloadFinished,
                                      character,
                                      SoundConstants.VolumeWeapon);
            }
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

        [RemoteCallSettings(DeliveryMode.Unreliable, maxCallsPerSecond: 30, keyArgIndex: 0)]
        private void ClientRemote_OnOtherCharacterReloaded(ICharacter character, IProtoItemWeapon protoWeapon)
        {
            protoWeapon.SoundPresetWeapon.PlaySound(WeaponSound.ReloadFinished,
                                                    character,
                                                    SoundConstants.VolumeWeapon);
        }

        [RemoteCallSettings(DeliveryMode.Unreliable, maxCallsPerSecond: 30, keyArgIndex: 0)]
        private void ClientRemote_OnOtherCharacterReloading(ICharacter character, IProtoItemWeapon protoWeapon)
        {
            protoWeapon.SoundPresetWeapon.PlaySound(WeaponSound.Reload,
                                                    character,
                                                    SoundConstants.VolumeWeapon);
        }

        private void ServerRemote_AbortReloading(IItem weapon)
        {
            var character = ServerRemoteContext.Character;
            SharedTryAbortReloading(character, weapon);
        }

        private void ServerRemote_ReloadWeapon(ReloadWeaponRequest args)
        {
            var character = ServerRemoteContext.Character;
            // force re-select current item
            PlayerCharacter.SharedForceRefreshCurrentItem(character);

            var itemWeapon = args.Item;
            if (itemWeapon is null)
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
            if (weaponState is null
                || weaponState.ItemWeapon != itemWeapon)
            {
                throw new Exception(
                    $"Only current active weapon could be reloaded: want to reload {itemWeapon}, but current active weapon is {weaponState?.ItemWeapon}");
            }

            var selectedProtoItemAmmo = args.ProtoItemAmmo;
            var ammoCurrent = privateState.AmmoCount;
            var ammoMax = itemProto.AmmoCapacity;

            if (weaponState.WeaponReloadingState is null
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
                SharedProcessWeaponReload(character, weaponState, out _);
            }
            else if (IsServer)
            {
                ServerNotifyAboutReloading(character, weaponState, isFinished: false);
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