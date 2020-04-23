namespace AtomicTorch.CBND.CoreMod.Systems.Deconstruction
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class DeconstructionSystem : ProtoSystem<DeconstructionSystem>
    {
        public const string NotificationCannotDeconstruct_Title = "Cannot deconstruct";

        public static event Action<DeconstructionActionState> ServerStructureDeconstructed;

        public override string Name => "Structures deconstruction system";

        public static bool ClientTryAbortAction()
        {
            var privateState = ClientCurrentCharacterHelper.PrivateState;
            var actionState = privateState.CurrentActionState as DeconstructionActionState;
            if (actionState == null)
            {
                return false;
            }

            privateState.SetCurrentActionState(null);
            return true;
        }

        public static void ClientTryStartAction()
        {
            var worldObject = ClientWorldObjectInteractHelper.ClientFindWorldObjectAtCurrentMousePosition();
            if (!(worldObject?.ProtoGameObject is IProtoObjectStructure))
            {
                return;
            }

            var currentCharacter = Client.Characters.CurrentPlayerCharacter;
            var privateState = PlayerCharacter.GetPrivateState(currentCharacter);

            if (privateState.CurrentActionState is DeconstructionActionState actionState
                && actionState.WorldObject == worldObject)
            {
                // the same object is already Deconstruction
                return;
            }

            SharedStartAction(currentCharacter, worldObject);
        }

        public static void SharedAbortAction(
            ICharacter character,
            IWorldObject worldObject)
        {
            if (worldObject == null)
            {
                return;
            }

            var characterPrivateState = PlayerCharacter.GetPrivateState(character);

            var actionState = characterPrivateState.CurrentActionState as DeconstructionActionState;
            if (actionState == null
                || actionState.WorldObject != worldObject)
            {
                // not deconstructing or deconstructing another object
                return;
            }

            if (!actionState.IsCompleted)
            {
                actionState.Cancel();
                return;
            }

            if (IsClient && worldObject.IsDestroyed)
            {
                // apparently the building finished deconstruction before the client simulation was complete
                SharedActionCompleted(character, actionState);
                return;
            }

            characterPrivateState.SetCurrentActionState(null);

            Logger.Important($"Deconstruction cancelled: {worldObject} by {character}", character);

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

        public static void SharedActionCompleted(ICharacter character, DeconstructionActionState state)
        {
            var worldObject = state.WorldObject;

            Logger.Important($"Deconstruction completed: {worldObject} by {character}", character);

            var characterPrivateState = PlayerCharacter.GetPrivateState(character);
            if (characterPrivateState.CurrentActionState != state)
            {
                Logger.Error("Should be impossible - current action state state doesn't match");
                return;
            }

            characterPrivateState.SetCurrentActionState(null);

            if (IsServer)
            {
                Api.SafeInvoke(() => ServerStructureDeconstructed?.Invoke(state));
            }
        }

        public static bool SharedCheckCanInteract(
            ICharacter character,
            IWorldObject worldObject,
            bool writeToLog)
        {
            if (worldObject == null
                || worldObject.IsDestroyed)
            {
                return false;
            }

            var staticWorldObject = (IStaticWorldObject)worldObject;
            return ConstructionSystem.CheckCanInteractForConstruction(character,
                                                                      staticWorldObject,
                                                                      writeToLog,
                                                                      checkRaidblock: true);
        }

        private static void SharedStartAction(ICharacter character, IWorldObject worldObject)
        {
            if (worldObject == null)
            {
                return;
            }

            if (!SharedCheckCanInteract(character, worldObject, writeToLog: true))
            {
                return;
            }

            if (!(worldObject.ProtoGameObject is IProtoObjectStructure))
            {
                throw new Exception("Not a structure: " + worldObject);
            }

            var characterPrivateState = PlayerCharacter.GetPrivateState(character);
            var characterPublicState = PlayerCharacter.GetPublicState(character);

            if (characterPrivateState.CurrentActionState is DeconstructionActionState actionState
                && actionState.WorldObject == worldObject)
            {
                // already deconstructing
                return;
            }

            var selectedHotbarItem = characterPublicState.SelectedItem;
            if (!(selectedHotbarItem?.ProtoGameObject is IProtoItemToolCrowbar))
            {
                selectedHotbarItem = null;
                if (!(worldObject.ProtoWorldObject is ProtoObjectConstructionSite))
                {
                    // no crowbar tool is selected, only construction sites
                    // can be deconstructed without the crowbar
                    return;
                }
            }

            actionState = new DeconstructionActionState(character, (IStaticWorldObject)worldObject, selectedHotbarItem);
            if (!actionState.CheckIsAllowed())
            {
                // not allowed to deconstruct
                Logger.Warning(
                    $"Deconstruction is not allowed: {worldObject} by {character}",
                    character);

                if (Api.IsClient)
                {
                    NotificationSystem.ClientShowNotification(
                        NotificationCannotDeconstruct_Title,
                        LandClaimSystem.ErrorNotLandOwner_Message,
                        NotificationColor.Bad,
                        selectedHotbarItem?.ProtoItem.Icon);
                }

                return;
            }

            if (!actionState.CheckIsNeeded())
            {
                // action is not needed
                Logger.Important($"Deconstruction is not required: {worldObject} by {character}", character);
                return;
            }

            characterPrivateState.SetCurrentActionState(actionState);

            Logger.Important($"Deconstruction started: {worldObject} by {character}", character);

            if (IsClient)
            {
                // TODO: display crowbar started animation? Send animation to other players?
                Instance.CallServer(_ => _.ServerRemote_StartAction(worldObject));
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_Cancel(IWorldObject worldObject)
        {
            var character = Client.Characters.CurrentPlayerCharacter;
            var characterPrivateState = PlayerCharacter.GetPrivateState(character);

            var actionState = characterPrivateState.CurrentActionState as DeconstructionActionState;
            if (actionState == null
                || actionState.WorldObject != worldObject)
            {
                // not deconstructing or deconstructing another object
                return;
            }

            // cancel action
            // TODO: fix this (possible) scenario: client cancel -> RPC -> server cancel -> RPC -> client cancel
            // TODO: fix this (possible) scenario: server cancel -> RPC -> client cancel -> RPC -> server cancel
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