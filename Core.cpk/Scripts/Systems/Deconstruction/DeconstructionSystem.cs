namespace AtomicTorch.CBND.CoreMod.Systems.Deconstruction
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class DeconstructionSystem : ProtoSystem<DeconstructionSystem>
    {
        public const string NotificationNotLandOwner_Message = "You're not the land area owner.";

        public const string NotificationNotLandOwner_Title = "Cannot deconstruct";

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
            if (!(worldObject?.ProtoStaticWorldObject is IProtoObjectStructure))
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

            // Can deconstruct only if the character can contact directly
            // with the object click area (if object has the click area)
            // or with the object collider (if object don't has the click area).
            var isObjectHasClickArea = worldObject.PhysicsBody
                                                  .Shapes
                                                  .Any(s => s.CollisionGroup == CollisionGroups.ClickArea);

            var result = worldObject.ProtoWorldObject.SharedIsInsideCharacterInteractionArea(
                character,
                worldObject,
                writeToLog: false,
                requiredCollisionGroup: isObjectHasClickArea
                                            ? CollisionGroups.ClickArea
                                            : CollisionGroups
                                                .DefaultWithCollisionToInteractionArea);
            if (result)
            {
                return true;
            }

            if (writeToLog)
            {
                Logger.Warning(
                    $"Cannot deconstruct - {character} cannot interact with the {worldObject} - there is no direct (physics) line of sight between them (the object click area or static/dynamic colliders must be inside the character interaction area)");

                if (IsClient)
                {
                    CannotInteractMessageDisplay.ShowOn(worldObject, CoreStrings.Notification_TooFar);
                    worldObject.ProtoWorldObject.SharedGetObjectSoundPreset()
                               .PlaySound(ObjectSound.InteractOutOfRange);
                }
            }

            return false;
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

            var selectedHotbarItem = characterPublicState.SelectedHotbarItem;
            if (!(selectedHotbarItem?.ProtoGameObject is IProtoItemToolCrowbar))
            {
                selectedHotbarItem = null;
                if (!(worldObject.ProtoWorldObject is ProtoObjectConstructionSite))
                {
                    // no crowbar tool is selected, only construction sites can be deconstructed without the crowbar
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
                        NotificationNotLandOwner_Title,
                        NotificationNotLandOwner_Message,
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