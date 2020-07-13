namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
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

        public const string DialogCannotSetOwners_MessageCannotRemoveSelf =
            "You cannot remove yourself from the owners list. Please ask another player to do so.";

        public const string DialogCannotSetOwners_MessageFormatPlayerNotFound
            = "Player with name [b]{0}[/b] is not found.";

        public const string DialogCannotSetOwners_MessageNotOwner
            = "You're not the owner.";

        public static event Action<IWorldObject> ServerOwnersChanged;

        public override string Name => "World object owners system";

        public static void ClientOnCannotInteractNotOwner(IWorldObject worldObject)
        {
            CannotInteractMessageDisplay.ClientOnCannotInteract(worldObject,
                                                                DialogCannotSetOwners_MessageNotOwner,
                                                                isOutOfRange: false);
        }

        public static async void ClientSetOwners(IWorldObject worldObject, List<string> newOwners)
        {
            var errorMessage = await Instance.CallServer(_ => _.ServerRemote_SetOwners(worldObject, newOwners));
            if (errorMessage != null)
            {
                NotificationSystem.ClientShowNotification(
                    title: null,
                    message: errorMessage,
                    color: NotificationColor.Bad);
            }
        }

        public static void ServerInitialize(IWorldObject worldObject)
        {
            var privateState = GetPrivateState(worldObject);
            if (privateState.Owners == null)
            {
                privateState.Owners = new NetworkSyncList<string>();
            }
        }

        public static void ServerNotifyNotOwner(ICharacter character, IWorldObject worldObject)
        {
            Logger.Warning(
                $"Character cannot interact with {worldObject} - not the owner.",
                character);

            Instance.CallClient(
                character,
                _ => _.ClientRemote_OnCannotInteractNotOwner(worldObject));
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
            ServerInvokeOwnersChangedEvent(structure);
        }

        public static string ServerSetOwners(
            IWorldObject worldObject,
            List<string> newOwners,
            NetworkSyncList<string> currentOwners,
            ICharacter byOwner)
        {
            var protoObject = (IProtoObjectWithOwnersList)worldObject.ProtoGameObject;
            if (!protoObject.HasOwnersList)
            {
                throw new Exception("This object doesn't support owners list: " + worldObject);
            }

            if (!InteractionCheckerSystem.SharedHasInteraction(ServerRemoteContext.Character,
                                                               worldObject,
                                                               requirePrivateScope: true))
            {
                throw new Exception("The player character is not interacting with " + worldObject);
            }

            if (!currentOwners.Contains(byOwner.Name)
                && !CreativeModeSystem.SharedIsInCreativeMode(byOwner))
            {
                return DialogCannotSetOwners_MessageNotOwner;
            }

            if (!((IProtoObjectWithOwnersList)worldObject.ProtoGameObject)
                    .SharedCanEditOwners(worldObject, byOwner))
            {
                return DialogCannotSetOwners_MessageCannotEdit;
            }

            currentOwners.GetDiff(newOwners, out var ownersToAdd, out var ownersToRemove);
            if (ownersToRemove.Count > 0
                && currentOwners.Count == ownersToRemove.Count)
            {
                return DialogCannotSetOwners_MessageCannotRemoveLastOwner;
            }

            if (ownersToRemove.Contains(byOwner.Name))
            {
                return DialogCannotSetOwners_MessageCannotRemoveSelf;
            }

            if (ownersToAdd.Count == 0
                && ownersToRemove.Count == 0)
            {
                Logger.Warning(
                    "No need to change the owners - the new owners list is the same as the current owners list: "
                    + worldObject,
                    characterRelated: byOwner);
                return null;
            }

            foreach (var n in ownersToAdd)
            {
                var name = n;
                var playerToAdd = Api.Server.Characters.GetPlayerCharacter(name);
                if (playerToAdd == null)
                {
                    return string.Format(DialogCannotSetOwners_MessageFormatPlayerNotFound, name);
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
                if (removedPlayer == null)
                {
                    continue;
                }

                InteractableWorldObjectHelper.ServerTryAbortInteraction(removedPlayer, worldObject);
            }

            ServerInvokeOwnersChangedEvent(worldObject);
            return null;
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

            if (SharedIsOwner(character, worldObject)
                || CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return true;
            }

            // not an owner
            if (writeToLog)
            {
                ServerNotifyNotOwner(character, worldObject);
            }

            return false;
        }

        public static IReadOnlyList<string> SharedGetOwners(IWorldObject worldObject)
        {
            return GetPrivateState(worldObject).Owners;
        }

        public static bool SharedIsOwner(ICharacter who, IWorldObject worldObject)
        {
            return SharedGetOwners(worldObject)
                .Contains(who.Name);
        }

        public byte ServerRemote_GetDoorOwnersMax()
        {
            return StructureConstants.SharedDoorOwnersMax;
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

        private static void ServerInvokeOwnersChangedEvent(IWorldObject worldObject)
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

        private static void ServerPlayerNameChangedHandler(string oldName, string newName)
        {
            var worldObjectsWithOwnerLists = Server.World.GetWorldObjectsOfProto<IProtoObjectWithOwnersList>();
            foreach (var worldObject in worldObjectsWithOwnerLists)
            {
                var owners = GetPrivateState(worldObject).Owners;
                if (owners == null)
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

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_OnCannotInteractNotOwner(IWorldObject worldObject)
        {
            ClientOnCannotInteractNotOwner(worldObject);
        }

        private string ServerRemote_SetOwners(IWorldObject worldObject, List<string> newOwners)
        {
            var maxOwners = worldObject.ProtoGameObject is IProtoObjectDoor
                                ? StructureConstants.SharedDoorOwnersMax
                                : byte.MaxValue;
            
            if (newOwners.Count > maxOwners)
            {
                return DialogCannotSetOwners_AccessListSizeLimitExceeded;
            }

            var owner = ServerRemoteContext.Character;
            var currentOwners = GetPrivateState(worldObject).Owners;
            return ServerSetOwners(worldObject, newOwners, currentOwners, owner);
        }
    }
}