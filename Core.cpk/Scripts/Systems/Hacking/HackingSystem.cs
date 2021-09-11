namespace AtomicTorch.CBND.CoreMod.Systems.Hacking
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class HackingSystem : ProtoSystem<HackingSystem>
    {
        public override string Name => "Hacking system";

        public static bool ClientTryAbortAction()
        {
            var privateState = ClientCurrentCharacterHelper.PrivateState;
            var actionState = privateState.CurrentActionState as HackingActionState;
            if (actionState is null)
            {
                return false;
            }

            privateState.SetCurrentActionState(null);
            return true;
        }

        public static void ClientTryStartAction()
        {
            var worldObject = ClientWorldObjectInteractHelper.ClientFindWorldObjectAtCurrentMousePosition();
            if (worldObject?.ProtoGameObject is not IProtoObjectHackableContainer)
            {
                return;
            }

            var currentCharacter = Client.Characters.CurrentPlayerCharacter;
            var privateState = PlayerCharacter.GetPrivateState(currentCharacter);

            if (privateState.CurrentActionState is HackingActionState actionState
                && actionState.WorldObject == worldObject)
            {
                // the same object is already Hacking
                return;
            }

            SharedStartAction(currentCharacter, worldObject);
        }

        public static void SharedAbortAction(
            ICharacter character,
            IWorldObject worldObject)
        {
            if (worldObject is null)
            {
                return;
            }

            var characterPrivateState = PlayerCharacter.GetPrivateState(character);

            var actionState = characterPrivateState.CurrentActionState as HackingActionState;
            if (actionState is null
                || actionState.WorldObject != worldObject)
            {
                // not hacking or hacking another object
                return;
            }

            if (!actionState.IsCompleted)
            {
                actionState.Cancel();
                return;
            }

            if (IsClient && worldObject.IsDestroyed)
            {
                // apparently the building finished hacking before the client simulation was complete
                SharedActionCompleted(character, actionState);
                return;
            }

            characterPrivateState.SetCurrentActionState(null);

            Logger.Important($"Hacking cancelled: {worldObject} by {character}", character);

            if (IsClient)
            {
                Instance.CallServer(_ => _.ServerRemote_Cancel(worldObject));
            }
            else // if Server
            {
                if (!ServerRemoteContext.IsRemoteCall)
                {
                    Instance.CallClient(character, _ => _.ClientRemote_Cancel(worldObject));
                    // TODO: notify other players as well
                }
            }
        }

        public static void SharedActionCompleted(ICharacter character, HackingActionState state)
        {
            var worldObject = state.WorldObject;

            Logger.Important($"Hacking completed: {worldObject} by {character}", character);

            var characterPrivateState = PlayerCharacter.GetPrivateState(character);
            if (characterPrivateState.CurrentActionState != state)
            {
                Logger.Error("Should be impossible - current action state state doesn't match");
                return;
            }

            characterPrivateState.SetCurrentActionState(null);
        }

        public static bool SharedCheckCanInteract(
            ICharacter character,
            IWorldObject worldObject,
            bool writeToLog)
        {
            if (worldObject is null
                || worldObject.IsDestroyed)
            {
                return false;
            }

            var staticWorldObject = (IStaticWorldObject)worldObject;
            if (!staticWorldObject.ProtoStaticWorldObject.SharedCanInteract(character,
                                                                            worldObject,
                                                                            writeToLog))
            {
                return false;
            }

            using var tempCharactersNearby = Api.Shared.GetTempList<ICharacter>();
            if (IsClient)
            {
                Client.Characters.GetKnownPlayerCharacters(tempCharactersNearby);
            }
            else
            {
                Server.World.GetScopedByPlayers(staticWorldObject, tempCharactersNearby);
            }

            foreach (var otherPlayerCharacter in tempCharactersNearby.AsList())
            {
                if (ReferenceEquals(character, otherPlayerCharacter))
                {
                    continue;
                }

                if (PlayerCharacter.GetPublicState(otherPlayerCharacter).CurrentPublicActionState
                        is HackingActionState.PublicState hackingState
                    && ReferenceEquals(hackingState.TargetWorldObject, staticWorldObject))
                {
                    // already hacking by another player
                    if (!writeToLog)
                    {
                        return false;
                    }

                    Logger.Important($"Cannot start hacking {staticWorldObject} - already hacking by another player",
                                     character);
                    if (IsClient)
                    {
                        NotificationSystem.ClientShowNotification(
                            CoreStrings.Notification_ErrorCannotInteract,
                            CoreStrings.Notification_ErrorObjectUsedByAnotherPlayer,
                            NotificationColor.Bad,
                            icon: staticWorldObject.ProtoStaticWorldObject.Icon);
                    }

                    return false;
                }
            }

            return true;
        }

        private static void SharedStartAction(ICharacter character, IWorldObject worldObject)
        {
            if (worldObject is null)
            {
                return;
            }

            if (!SharedCheckCanInteract(character, worldObject, writeToLog: true))
            {
                return;
            }

            if (worldObject.ProtoGameObject is not IProtoObjectHackableContainer)
            {
                throw new Exception("Not a hackable container: " + worldObject);
            }

            var characterPrivateState = PlayerCharacter.GetPrivateState(character);

            if (characterPrivateState.CurrentActionState is HackingActionState actionState
                && actionState.WorldObject == worldObject)
            {
                // already hacking
                return;
            }

            actionState = new HackingActionState(character, (IStaticWorldObject)worldObject);

            if (!actionState.CheckIsNeeded())
            {
                // action is not needed
                Logger.Important($"Hacking is not required: {worldObject} by {character}", character);
                return;
            }

            characterPrivateState.SetCurrentActionState(actionState);

            Logger.Important($"Hacking started: {worldObject} by {character}", character);

            if (IsClient)
            {
                Instance.CallServer(_ => _.ServerRemote_StartAction(worldObject));
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_Cancel(IWorldObject worldObject)
        {
            var character = Client.Characters.CurrentPlayerCharacter;
            var characterPrivateState = PlayerCharacter.GetPrivateState(character);

            var actionState = characterPrivateState.CurrentActionState as HackingActionState;
            if (actionState is null
                || actionState.WorldObject != worldObject)
            {
                // not hacking or hacking another object
                return;
            }

            // cancel action
            characterPrivateState.SetCurrentActionState(null);
        }

        private void ServerRemote_Cancel(IWorldObject worldObject)
        {
            var character = ServerRemoteContext.Character;
            SharedAbortAction(character, worldObject);
        }

        private void ServerRemote_StartAction(IWorldObject worldObject)
        {
            var character = ServerRemoteContext.Character;
            SharedStartAction(character, worldObject);
        }
    }
}