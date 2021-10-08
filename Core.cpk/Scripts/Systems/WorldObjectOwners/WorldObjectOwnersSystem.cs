namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class WorldObjectOwnersSystem : ProtoSystem<WorldObjectOwnersSystem>
    {
        public const string DialogCannotSetOwners_AccessListSizeLimitExceeded
            = "Access list size limit exceeded.";

        public const string DialogCannotSetOwners_MessageCannotEdit
            = "You cannot edit the owners list.";

        public const string DialogCannotSetOwners_MessageCannotRemoveLastOwner
            = "You cannot remove the last owner.";

        public const string DialogCannotSetOwners_MessageCannotRemoveSelf
            = "You cannot remove yourself from the owners list. Please ask another player to do so.";

        public const string DialogCannotSetOwners_MessageNotOwner
            = "You're not the owner.";

        public static event Action<IWorldObject> ServerOwnersChanged;

        [RemoteEnum]
        public enum SetOwnersResult : byte
        {
            Success,

            [Description(CoreStrings.PlayerNotFound)]
            ErrorPlayerNotFound,

            [Description(DialogCannotSetOwners_MessageNotOwner)]
            ErrorNotOwner,

            [Description(DialogCannotSetOwners_MessageCannotEdit)]
            ErrorCannotEdit,

            [Description(DialogCannotSetOwners_MessageCannotRemoveSelf)]
            ErrorCannotRemoveSelf,

            [Description(DialogCannotSetOwners_MessageCannotRemoveLastOwner)]
            ErrorCannotRemoveLastOwner,

            [Description(DialogCannotSetOwners_AccessListSizeLimitExceeded)]
            ErrorAccessListSizeLimitExceeded
        }

        public static void ClientOnCannotInteractNotOwner(IWorldObject worldObject, bool isFactionAccess)
        {
            var message = isFactionAccess
                              ? WorldObjectAccessModeSystem.NotificationDontHaveAccess
                              : DialogCannotSetOwners_MessageNotOwner;
            CannotInteractMessageDisplay.ClientOnCannotInteract(worldObject,
                                                                message,
                                                                isOutOfRange: false);
        }

        public static async void ClientSetOwners(IWorldObject worldObject, List<string> newOwners)
        {
            var result = await Instance.CallServer(_ => _.ServerRemote_SetOwners(worldObject, newOwners));
            if (result != SetOwnersResult.Success)
            {
                NotificationSystem.ClientShowNotification(
                    title: null,
                    message: result.GetDescription(),
                    color: NotificationColor.Bad);
            }
        }

        public static void ServerInitialize(IWorldObject worldObject)
        {
            var privateState = GetPrivateState(worldObject);
            privateState.Owners ??= new NetworkSyncList<string>();
        }

        public static void ServerNotifyNotOwner(ICharacter character, IWorldObject worldObject, bool isFactionAccess)
        {
            Logger.Warning(
                $"Character cannot interact with {worldObject} - not the owner.",
                character);

            Instance.CallClient(
                character,
                _ => _.ClientRemote_OnCannotInteractNotOwner(worldObject, isFactionAccess));
        }

        public static void ServerOnBuilt(IWorldObject structure, ICharacter byCharacter)
        {
            var protoObject = (IProtoObjectWithOwnersList)structure.ProtoGameObject;
            if (!protoObject.HasOwnersList)
            {
                return;
            }

            // add the player character to the owners list
            GetPrivateState(structure).Owners.Add(byCharacter.Name);
            ServerOnOwnersChanged(structure);
        }

        public static void ServerOnOwnersChanged(IWorldObject worldObject)
        {
            try
            {
                ServerOwnersChanged?.Invoke(worldObject);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, $"Exception during the {nameof(ServerOwnersChanged)} event call");
            }
        }

        public static SetOwnersResult ServerSetOwners(
            IWorldObject worldObject,
            List<string> newOwners,
            ICharacter byOwner)
        {
            var protoObject = (IProtoObjectWithOwnersList)worldObject.ProtoGameObject;
            if (!protoObject.HasOwnersList)
            {
                throw new Exception("This object doesn't support owners list: " + worldObject);
            }

            var currentOwners = GetPrivateState(worldObject).Owners;
            if (!currentOwners.Contains(byOwner.Name)
                && !CreativeModeSystem.SharedIsInCreativeMode(byOwner))
            {
                return SetOwnersResult.ErrorNotOwner;
            }

            if (!((IProtoObjectWithOwnersList)worldObject.ProtoGameObject)
                    .SharedCanEditOwners(worldObject, byOwner))
            {
                return SetOwnersResult.ErrorCannotEdit;
            }

            currentOwners.GetDiff(newOwners, out var ownersToAdd, out var ownersToRemove);
            if (ownersToRemove.Count > 0
                && currentOwners.Count == ownersToRemove.Count)
            {
                return SetOwnersResult.ErrorCannotRemoveLastOwner;
            }

            if (ownersToRemove.Contains(byOwner.Name))
            {
                return SetOwnersResult.ErrorCannotRemoveSelf;
            }

            if (ownersToAdd.Count == 0
                && ownersToRemove.Count == 0)
            {
                Logger.Warning(
                    "No need to change the owners - the new owners list is the same as the current owners list: "
                    + worldObject,
                    characterRelated: byOwner);
                return SetOwnersResult.Success;
            }

            foreach (var n in ownersToAdd)
            {
                var name = n;
                var playerToAdd = Api.Server.Characters.GetPlayerCharacter(name);
                if (playerToAdd is null)
                {
                    return SetOwnersResult.ErrorPlayerNotFound;
                }

                // get proper player name
                name = playerToAdd.Name;
                if (currentOwners.AddIfNotContains(name))
                {
                    Api.Logger.Important($"Added owner: {name}; {worldObject}", characterRelated: byOwner);
                }
            }

            foreach (var name in ownersToRemove)
            {
                if (!currentOwners.Remove(name))
                {
                    continue;
                }

                Api.Logger.Important($"Removed owner: {name}; {worldObject}", characterRelated: byOwner);

                var removedPlayer = Api.Server.Characters.GetPlayerCharacter(name);
                if (removedPlayer is null)
                {
                    continue;
                }

                InteractableWorldObjectHelper.ServerTryAbortInteraction(removedPlayer, worldObject);
            }

            ServerOnOwnersChanged(worldObject);
            return SetOwnersResult.Success;
        }

        public static bool SharedCanInteract(
            ICharacter character,
            IWorldObject worldObject,
            bool writeToLog)
        {
            if (IsClient)
            {
                // cannot perform this check on the client side
                return true;
            }

            if (SharedIsOwner(character, worldObject, out var isFactionAccess)
                || CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return true;
            }

            // not an owner
            if (writeToLog)
            {
                ServerNotifyNotOwner(character, worldObject, isFactionAccess);
            }

            return false;
        }

        /// <summary>
        /// Gets the direct owners of the world object.
        /// Unlike SharedGetOwners method, in the case of faction ownership,
        /// faction owners will be not provided.
        /// </summary>
        public static IReadOnlyList<string> SharedGetDirectOwners(IWorldObject worldObject)
        {
            return GetPrivateState(worldObject).Owners;
        }

        public static IReadOnlyList<string> SharedGetOwners(IWorldObject worldObject, out bool isFactionAccess)
        {
            isFactionAccess = false;

            switch (worldObject)
            {
                case IStaticWorldObject staticWorldObject:
                {
                    var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(staticWorldObject);
                    if (areasGroup is null
                        || LandClaimAreasGroup.GetPublicState(areasGroup).FactionClanTag is not { } clanTag
                        || string.IsNullOrEmpty(clanTag))
                    {
                        break;
                    }

                    // the static object is inside the faction-owned land claim,
                    var faction = FactionSystem.ServerGetFactionByClanTag(clanTag);
                    return FactionSystem.ServerGetFactionMemberNames(faction).ToList();
                }

                case IDynamicWorldObject when worldObject.ProtoGameObject is IProtoVehicle:
                {
                    var clanTag = worldObject.GetPublicState<VehiclePublicState>().ClanTag;
                    if (string.IsNullOrEmpty(clanTag))
                    {
                        break;
                    }

                    // the vehicle is owned by faction
                    var faction = FactionSystem.ServerGetFactionByClanTag(clanTag);
                    return FactionSystem.ServerGetFactionMemberNames(faction).ToList();
                }
            }

            return SharedGetDirectOwners(worldObject);
        }

        public static bool SharedIsOwner(ICharacter who, IWorldObject worldObject)
        {
            return SharedIsOwner(who, worldObject, out _);
        }

        public static bool SharedIsOwner(ICharacter who, IWorldObject worldObject, out bool isFactionAccess)
        {
            isFactionAccess = false;

            switch (worldObject)
            {
                case IStaticWorldObject staticWorldObject:
                {
                    var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(staticWorldObject);
                    if (areasGroup is null
                        || LandClaimAreasGroup.GetPublicState(areasGroup).FactionClanTag is not { } clanTag
                        || string.IsNullOrEmpty(clanTag))
                    {
                        break;
                    }

                    // the static object is inside the faction-owned land claim,
                    // verify permission
                    isFactionAccess = true;
                    return SharedHasFactionAccessRights(who,
                                                        FactionMemberAccessRights.LandClaimManagement,
                                                        clanTag);
                }

                case IDynamicWorldObject when worldObject.ProtoGameObject is IProtoVehicle:
                {
                    var clanTag = worldObject.GetPublicState<VehiclePublicState>().ClanTag;
                    if (string.IsNullOrEmpty(clanTag))
                    {
                        break;
                    }

                    // the vehicle is owned by faction
                    // verify whether the character has the access right
                    isFactionAccess = true;
                    return WorldObjectAccessModeSystem.SharedHasAccess(who, worldObject, writeToLog: true);
                }
            }

            return SharedGetDirectOwners(worldObject).Contains(who.Name);
        }

        protected override void PrepareSystem()
        {
            if (IsServer)
            {
                Server.Characters.PlayerNameChanged += ServerPlayerNameChangedHandler;
            }
        }

        private static IObjectWithOwnersPrivateState GetPrivateState(IWorldObject structure)
        {
            return structure.GetPrivateState<IObjectWithOwnersPrivateState>();
        }

        private static void ServerPlayerNameChangedHandler(string oldName, string newName)
        {
            var worldObjectsWithOwnerLists = Server.World.GetWorldObjectsOfProto<IProtoObjectWithOwnersList>();
            foreach (var worldObject in worldObjectsWithOwnerLists)
            {
                var owners = GetPrivateState(worldObject).Owners;
                if (owners is null)
                {
                    continue;
                }

                for (var index = 0; index < owners.Count; index++)
                {
                    var owner = owners[index];
                    if (!string.Equals(owner, oldName, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    // replace owner entry
                    owners.RemoveAt(index);
                    owners.Insert(index, newName);
                    Logger.Important($"Replaced owner entry: {oldName}->{newName} in {worldObject}");
                    break;
                }
            }
        }

        private static bool SharedHasFactionAccessRights(
            ICharacter who,
            FactionMemberAccessRights accessRights,
            string clanTag)
        {
            Api.Assert(!string.IsNullOrEmpty(clanTag), "Clan tag is null or empty");

            if (clanTag != FactionSystem.SharedGetClanTag(who))
            {
                // player has no faction or a member of a different faction
                return false;
            }

            return FactionSystem.SharedHasAccessRight(who, accessRights);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_OnCannotInteractNotOwner(IWorldObject worldObject, bool isFactionAccess)
        {
            ClientOnCannotInteractNotOwner(worldObject, isFactionAccess);
        }

        [RemoteCallSettings(timeInterval: 1)]
        private SetOwnersResult ServerRemote_SetOwners(IWorldObject worldObject, List<string> newOwners)
        {
            var maxOwners = worldObject.ProtoGameObject is IProtoObjectDoor
                                ? RateDoorOwnersMax.SharedValue
                                : byte.MaxValue;

            if (newOwners.Count > maxOwners)
            {
                return SetOwnersResult.ErrorAccessListSizeLimitExceeded;
            }

            var character = ServerRemoteContext.Character;
            if (worldObject is IStaticWorldObject staticWorldObject)
            {
                if (!staticWorldObject.ProtoStaticWorldObject
                                      .SharedCanInteract(character, worldObject, writeToLog: true))
                {
                    throw new Exception("Cannot interact with " + worldObject);
                }

                if (!SharedIsOwner(character, worldObject))
                {
                    throw new Exception("Not an owner");
                }

                var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(staticWorldObject);
                if (areasGroup is not null
                    && LandClaimAreasGroup.GetPublicState(areasGroup).ServerFaction is not null)
                {
                    throw new Exception(
                        "Cannot modify owners list for an object within a faction land claim area");
                }
            }
            else // dynamic world object (a vehicle, etc)
            {
                if (!worldObject.ProtoWorldObject.SharedCanInteract(character, worldObject, writeToLog: false))
                {
                    throw new Exception("Cannot interact with " + worldObject);
                }
            }

            return ServerSetOwners(worldObject, newOwners, byOwner: character);
        }
    }
}