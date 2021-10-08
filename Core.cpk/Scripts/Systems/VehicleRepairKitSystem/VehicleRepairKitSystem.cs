namespace AtomicTorch.CBND.CoreMod.Systems.VehicleRepairKitSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Special;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class VehicleRepairKitSystem : ProtoSystem<VehicleRepairKitSystem>
    {
        public const string NotificationErrorCannotRepairThisVehicle = "Cannot repair this vehicle";

        public const double RepairStageDurationSeconds = 10;

        private const double MaxDistanceForRepairAction = 2.24;

        public static IDynamicWorldObject ClientGetObjectToRepairAtCurrentMousePosition(
            bool showErrorIfNoCompatibleVehicle)
        {
            var worldObjects = ClientGetObjectsAtCurrentMousePosition();
            IDynamicWorldObject vehicleToRepair = null;
            IProtoVehicle protoVehicleIncompatible = null;

            // find first damaged vehicle there
            foreach (var worldObject in worldObjects)
            {
                if (worldObject.ProtoGameObject is not IProtoVehicle protoVehicle)
                {
                    continue;
                }

                if (!protoVehicle.IsRepairable)
                {
                    protoVehicleIncompatible = protoVehicle;
                    continue;
                }

                var vehicle = (IDynamicWorldObject)worldObject;
                var maxStructurePointsMax = protoVehicle.SharedGetStructurePointsMax(vehicle);
                var structurePointsCurrent = worldObject.GetPublicState<VehiclePublicState>()
                                                        .StructurePointsCurrent;
                if (structurePointsCurrent < maxStructurePointsMax)
                {
                    vehicleToRepair = vehicle;
                    break;
                }
            }

            if (vehicleToRepair is null
                && protoVehicleIncompatible is not null
                && showErrorIfNoCompatibleVehicle)
            {
                NotificationSystem.ClientShowNotification(NotificationErrorCannotRepairThisVehicle,
                                                          color: NotificationColor.Bad,
                                                          icon: protoVehicleIncompatible.Icon);
            }

            return vehicleToRepair;
        }

        public static bool ClientTryAbortAction()
        {
            var privateState = ClientCurrentCharacterHelper.PrivateState;
            var actionState = privateState.CurrentActionState as VehicleRepairActionState;
            if (actionState is null)
            {
                return false;
            }

            // cancel repair state
            privateState.SetCurrentActionState(null);
            return true;
        }

        public static void ClientTryStartAction()
        {
            if (ClientHotbarSelectedItemManager.SelectedItem?.ProtoGameObject
                    is not IProtoItemVehicleRepairKit protoItemVehicleRepairKit)
            {
                // no tool is selected
                return;
            }

            if (!ClientInputManager.IsButtonHeld(GameButton.ActionUseCurrentItem))
            {
                // the tool is not currently used
                return;
            }

            var vehicleToRepair = ClientGetObjectToRepairAtCurrentMousePosition(
                showErrorIfNoCompatibleVehicle: ClientInputManager.IsButtonDown(GameButton.ActionUseCurrentItem,
                    evenIfHandled: true));
            if (vehicleToRepair is null)
            {
                return;
            }

            var currentCharacter = Client.Characters.CurrentPlayerCharacter;
            var privateState = PlayerCharacter.GetPrivateState(currentCharacter);

            if (privateState.CurrentActionState is VehicleRepairActionState actionState
                && ReferenceEquals(actionState.Vehicle, vehicleToRepair))
            {
                // the same object is already repairing
                return;
            }

            SharedStartAction(currentCharacter, vehicleToRepair);
        }

        public static void SharedAbortAction(ICharacter character, IDynamicWorldObject vehicle)
        {
            if (vehicle is null)
            {
                return;
            }

            var characterPrivateState = PlayerCharacter.GetPrivateState(character);

            var actionState = characterPrivateState.CurrentActionState as VehicleRepairActionState;
            if (actionState is null
                || actionState.Vehicle != vehicle)
            {
                // not repairing or repairing another object
                return;
            }

            if (!actionState.IsCompleted)
            {
                actionState.Cancel();
                return;
            }

            if (IsClient
                && (vehicle.IsDestroyed
                    || actionState.VehiclePublicState.StructurePointsCurrent >= actionState.StructurePointsMax))
            {
                // apparently the system finished repair before the client simulation was complete
                // or the vehicle destroyed
                SharedActionCompleted(character, actionState);
                return;
            }

            characterPrivateState.SetCurrentActionState(null);

            Logger.Info($"Repairing cancelled: {vehicle} by {character}", character);

            if (IsClient)
            {
                Instance.CallServer(_ => _.ServerRemote_Cancel(vehicle));
            }
            else // if Server
            {
                if (!ServerRemoteContext.IsRemoteCall)
                {
                    Instance.CallClient(character, _ => _.ClientRemote_Cancel(vehicle));
                    // TODO: notify other players as well
                }
            }
        }

        public static void SharedActionCompleted(ICharacter character, VehicleRepairActionState state)
        {
            Logger.Info($"Repairing action completed: {state.Vehicle} by {character}", character);

            var characterPrivateState = PlayerCharacter.GetPrivateState(character);
            if (characterPrivateState.CurrentActionState != state)
            {
                throw new Exception("Should be impossible!");
            }

            characterPrivateState.SetCurrentActionState(null);
        }

        public static bool SharedCheckCanInteract(
            ICharacter character,
            IDynamicWorldObject vehicle,
            bool writeToLog)
        {
            if (vehicle is null
                || vehicle.IsDestroyed)
            {
                return false;
            }

            // it's possible to repair any vehicle within a certain distance to the character
            var canInteract = character.Position.DistanceSquaredTo(vehicle.Position)
                              <= MaxDistanceForRepairAction * MaxDistanceForRepairAction;

            if (!canInteract)
            {
                if (writeToLog)
                {
                    Logger.Warning(
                        $"Character cannot interact with {vehicle} for repair - too far",
                        character);

                    if (IsClient)
                    {
                        CannotInteractMessageDisplay.ClientOnCannotInteract(vehicle,
                                                                            CoreStrings.Notification_TooFar,
                                                                            isOutOfRange: true);
                    }
                }

                return false;
            }

            var physicsSpace = character.PhysicsBody.PhysicsSpace;
            var characterCenter = character.Position + character.PhysicsBody.CenterOffset;

            if (ObstacleTestHelper.SharedHasObstaclesInTheWay(characterCenter,
                                                              physicsSpace,
                                                              vehicle,
                                                              sendDebugEvents: writeToLog))
            {
                if (writeToLog)
                {
                    Logger.Warning(
                        $"Character cannot interact with {vehicle} for repair - obstacles in the way",
                        character);

                    if (IsClient)
                    {
                        CannotInteractMessageDisplay.ClientOnCannotInteract(vehicle,
                                                                            CoreStrings.Notification_ObstaclesOnTheWay,
                                                                            isOutOfRange: true);
                    }
                }

                return false;
            }

            using var tempCharactersNearby = Api.Shared.GetTempList<ICharacter>();
            if (IsClient)
            {
                Client.Characters.GetKnownPlayerCharacters(tempCharactersNearby);
            }
            else
            {
                Server.World.GetScopedByPlayers(vehicle, tempCharactersNearby);
            }

            foreach (var otherPlayerCharacter in tempCharactersNearby.AsList())
            {
                if (ReferenceEquals(character, otherPlayerCharacter))
                {
                    continue;
                }

                if (PlayerCharacter.GetPublicState(otherPlayerCharacter).CurrentPublicActionState
                        is VehicleRepairActionState.PublicState repairActionState
                    && ReferenceEquals(repairActionState.TargetWorldObject, vehicle))
                {
                    // already repairing by another player
                    if (!writeToLog)
                    {
                        return false;
                    }

                    Logger.Important($"Cannot start repairing {vehicle} - already repairing by another player",
                                     character);
                    if (IsClient)
                    {
                        NotificationSystem.ClientShowNotification(
                            CoreStrings.Notification_ErrorCannotInteract,
                            CoreStrings.Notification_ErrorObjectUsedByAnotherPlayer,
                            NotificationColor.Bad,
                            icon: ((IProtoVehicle)vehicle.ProtoGameObject).Icon);
                    }

                    return false;
                }
            }

            return true;
        }

        private static IEnumerable<IWorldObject> ClientGetObjectsAtCurrentMousePosition()
        {
            var currentCharacter = Api.Client.Characters.CurrentPlayerCharacter;
            return ClientComponentObjectInteractionHelper
                   // find by click area
                   .FindObjectsAtCurrentMousePosition(
                       currentCharacter,
                       CollisionGroups.ClickArea)
                   // find by default collider
                   .Concat(
                       ClientComponentObjectInteractionHelper
                           .FindObjectsAtCurrentMousePosition(
                               currentCharacter,
                               CollisionGroups.Default));
        }

        private static void SharedStartAction(ICharacter character, IDynamicWorldObject vehicle)
        {
            if (vehicle?.ProtoGameObject is not IProtoVehicle)
            {
                return;
            }

            var characterPrivateState = PlayerCharacter.GetPrivateState(character);
            var characterPublicState = PlayerCharacter.GetPublicState(character);

            if (characterPrivateState.CurrentActionState is VehicleRepairActionState actionState
                && actionState.Vehicle == vehicle)
            {
                // already repairing specified object
                return;
            }

            var selectedHotbarItem = characterPublicState.SelectedItem;
            if (selectedHotbarItem?.ProtoGameObject is not IProtoItemVehicleRepairKit)
            {
                // no tool is selected
                return;
            }

            actionState = new VehicleRepairActionState(character, vehicle, selectedHotbarItem);
            if (!actionState.CheckIsNeeded())
            {
                // action is not needed
                Logger.Info($"Repairing is not required: {vehicle} by {character}", character);
                return;
            }

            if (!SharedCheckCanInteract(character, vehicle, true))
            {
                return;
            }

            characterPrivateState.SetCurrentActionState(actionState);

            Logger.Info($"Repairing started: {vehicle} by {character}", character);

            if (IsClient)
            {
                // TODO: we need animation for repairing
                Instance.CallServer(_ => _.ServerRemote_StartAction(vehicle));
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_Cancel(IDynamicWorldObject vehicle)
        {
            var character = Client.Characters.CurrentPlayerCharacter;
            var characterPrivateState = PlayerCharacter.GetPrivateState(character);

            var actionState = characterPrivateState.CurrentActionState as VehicleRepairActionState;
            if (actionState is null
                || actionState.Vehicle != vehicle)
            {
                // not repairing or repairing another object
                return;
            }

            // cancel action
            characterPrivateState.SetCurrentActionState(null);
        }

        private void ServerRemote_Cancel(IDynamicWorldObject vehicle)
        {
            var character = ServerRemoteContext.Character;
            SharedAbortAction(character, vehicle);
        }

        private void ServerRemote_StartAction(IDynamicWorldObject vehicle)
        {
            var character = ServerRemoteContext.Character;
            SharedStartAction(character, vehicle);
        }
    }
}