namespace AtomicTorch.CBND.CoreMod.Systems.VehicleSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class VehicleSystem : ProtoSystem<VehicleSystem>
    {
        public const string Notification_CannotUseVehicle_TitleFormat = "Cannot use {0}";

        private static readonly Dictionary<ICharacter, IDynamicWorldObject> ServerVehicleQuitRequests
            = IsServer ? new Dictionary<ICharacter, IDynamicWorldObject>() : null;

        public override string Name => "Vehicle system";

        public static void ClientOnVehicleEnterOrExitRequest()
        {
            Instance.ClientOnVehicleEnterExitButtonPress();
        }

        public static void ServerCharacterExitCurrentVehicle(ICharacter character)
        {
            if (character.IsNpc)
            {
                return;
            }

            var vehicle = PlayerCharacter.GetPublicState(character).CurrentVehicle;
            if (vehicle is null)
            {
                return;
            }

            ServerCharacterExitVehicle(character, vehicle);
        }

        public static void ServerCharacterExitVehicle(ICharacter character, IDynamicWorldObject vehicle)
        {
            var characterPublicState = PlayerCharacter.GetPublicState(character);
            if (characterPublicState.CurrentVehicle != vehicle)
            {
                return;
            }

            var vehiclePublicState = vehicle.GetPublicState<VehiclePublicState>();
            if (vehiclePublicState.IsDismountRequested)
            {
                // quit is already requested
                return;
            }

            if (vehicle.PhysicsBody.Velocity.LengthSquared > 0)
            {
                // cannot quit now, check later
                ServerVehicleQuitRequests[character] = vehicle;
                vehiclePublicState.IsDismountRequested = true;
                return;
            }

            ServerQuitVehicleNow(character, vehicle);
        }

        public static void ServerForceExitVehicle(IDynamicWorldObject vehicle)
        {
            var vehiclePublicState = vehicle?.GetPublicState<VehiclePublicState>();
            if (vehiclePublicState?.PilotCharacter is null)
            {
                return;
            }

            ServerQuitVehicleNow(vehiclePublicState.PilotCharacter, vehicle);
        }

        public static void ServerOnVehicleDestroyed(IDynamicWorldObject vehicle)
        {
            var publicState = vehicle.GetPublicState<VehiclePublicState>();
            if (!(publicState.PilotCharacter is null))
            {
                ServerCharacterExitVehicle(publicState.PilotCharacter, vehicle);
            }

            ServerResetLastVehicleMapMark(
                vehicle.GetPrivateState<VehiclePrivateState>());
        }

        public static void ServerResetLastVehicleMapMark(VehiclePrivateState vehiclePrivateState)
        {
            if (vehiclePrivateState.ServerLastPilotCharacter is null)
            {
                return;
            }

            var characterPrivateState = PlayerCharacter.GetPrivateState(vehiclePrivateState.ServerLastPilotCharacter);
            characterPrivateState.LastDismountedVehicleMapMark = default;
            vehiclePrivateState.ServerLastPilotCharacter = null;
        }

        public static IStaticWorldObject SharedFindVehicleAssemblyBayNearby(ICharacter character)
        {
            using var tempStaticObjects = Api.Shared.GetTempList<IStaticWorldObject>();
            if (IsServer)
            {
                Server.World.GetStaticWorldObjectsInView(character,
                                                         tempStaticObjects,
                                                         sortByDistance: false);
            }
            else
            {
                Client.World.GetStaticWorldObjects(tempStaticObjects);
            }

            foreach (var staticWorldObject in tempStaticObjects)
            {
                if (staticWorldObject.ProtoStaticWorldObject is IProtoVehicleAssemblyBay protoVehicleAssemblyBay
                    && protoVehicleAssemblyBay.SharedCanInteract(character, staticWorldObject, writeToLog: false))
                {
                    return staticWorldObject;
                }
            }

            return null;
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                ClientInputContext.Start("Enter/Exit vehicle")
                                  .HandleButtonDown(GameButton.VehicleEnterExit,
                                                    this.ClientOnVehicleEnterExitButtonPress);
            }
            else
            {
                TriggerEveryFrame.ServerRegister(ServerProcessVehicleQuitRequests,
                                                 nameof(VehicleSystem));
            }
        }

        private static void ServerProcessVehicleQuitRequests()
        {
            ServerVehicleQuitRequests.ProcessAndRemoveByValue(
                removeCondition: v => v.IsDestroyed
                                      || v.PhysicsBody is null
                                      || v.PhysicsBody.Velocity.LengthSquared == 0,
                removeCallback: p => ServerQuitVehicleNow(character: p.Key,
                                                          vehicle: p.Value));
        }

        private static void ServerQuitVehicleNow(ICharacter character, IDynamicWorldObject vehicle)
        {
            if (ServerVehicleQuitRequests.TryGetValue(character, out var requestedVehicleToQuit))
            {
                ServerVehicleQuitRequests.Remove(character);
                requestedVehicleToQuit.GetPublicState<VehiclePublicState>()
                                      .IsDismountRequested = false;
            }

            var characterPublicState = PlayerCharacter.GetPublicState(character);
            if (characterPublicState.CurrentVehicle != vehicle)
            {
                return;
            }

            InteractableWorldObjectHelper.ServerTryAbortInteraction(character, vehicle);

            vehicle.GetPublicState<VehiclePublicState>()
                   .IsDismountRequested = false;

            characterPublicState.ServerSetCurrentVehicle(null);

            var vehiclePublicState = vehicle.GetPublicState<VehiclePublicState>();
            vehiclePublicState.PilotCharacter = null;
            Logger.Important("Player exit vehicle: " + vehicle, character);

            if (!vehicle.IsDestroyed)
            {
                var characterPrivateState = PlayerCharacter.GetPrivateState(character);
                characterPrivateState.LastDismountedVehicleMapMark = new LastDismountedVehicleMapMark(vehicle);
            }

            // rebuild physics
            character.ProtoWorldObject.SharedCreatePhysics(character);

            if (!vehicle.IsDestroyed)
            {
                vehicle.ProtoWorldObject.SharedCreatePhysics(vehicle);
                // ensure vehicle stopped
                Server.World.SetDynamicObjectPhysicsMovement(vehicle, Vector2D.Zero, targetVelocity: 0);
                Server.World.SetDynamicObjectMoveSpeed(vehicle, 0);
                Server.World.StopPhysicsBody(vehicle.PhysicsBody);
            }

            PlayerCharacter.SharedForceRefreshCurrentItem(character);
        }

        private void ClientOnVehicleEnterExitButtonPress()
        {
            var character = ClientCurrentCharacterHelper.Character;
            if (character is null)
            {
                return;
            }

            var vehicle = ClientCurrentCharacterHelper.PublicState.CurrentVehicle;
            if (vehicle != null)
            {
                // already inside a vehicle
                vehicle.GetPublicState<VehiclePublicState>()
                       .IsDismountRequested = true;
                this.CallServer(_ => _.ServerRemote_ExitVehicle(vehicle));
                return;
            }

            if (InteractionCheckerSystem.SharedGetCurrentInteraction(character) is IDynamicWorldObject
                    currentlyInteractingWithDynamicWorldObject
                && currentlyInteractingWithDynamicWorldObject.ProtoGameObject is IProtoVehicle)
            {
                vehicle = currentlyInteractingWithDynamicWorldObject;
            }
            else if (ClientComponentObjectInteractionHelper.MouseOverObject
                         is IDynamicWorldObject mouseOverWorldObject
                     && mouseOverWorldObject.ProtoGameObject is IProtoVehicle)
            {
                vehicle = mouseOverWorldObject;
            }

            if (vehicle is null)
            {
                // no vehicle in interaction or under mouse cursor - try find a vehicle nearby
                var closestVehicle = Client.World.GetGameObjectsOfProto<IDynamicWorldObject, IProtoVehicle>()
                                           .OrderBy(v => v.Position.DistanceSquaredTo(character.Position))
                                           .FirstOrDefault();
                if (closestVehicle != null
                    && closestVehicle.Position.DistanceTo(character.Position) < 3)
                {
                    vehicle = closestVehicle;
                }
            }

            if (vehicle is null)
            {
                // no vehicle to enter
                return;
            }

            if (!vehicle.ProtoWorldObject.SharedCanInteract(character, vehicle, writeToLog: true))
            {
                // cannot interact with this vehicle
                return;
            }

            WindowObjectVehicle.CloseActiveMenu();
            vehicle.GetPublicState<VehiclePublicState>()
                   .IsDismountRequested = false;
            this.CallServer(_ => _.ServerRemote_EnterVehicle(vehicle));
        }

        private void ServerRemote_EnterVehicle(IDynamicWorldObject vehicle)
        {
            var character = ServerRemoteContext.Character;
            if (!(vehicle.ProtoGameObject is IProtoVehicle protoVehicle))
            {
                Logger.Warning($"Player cannot enter vehicle: {vehicle} - it's not a vehicle!", character);
                return;
            }

            if (!vehicle.ProtoWorldObject.SharedCanInteract(character,
                                                            vehicle,
                                                            writeToLog: true))
            {
                return;
            }

            var characterPublicState = PlayerCharacter.GetPublicState(character);
            if (characterPublicState.CurrentVehicle != null)
            {
                Logger.Warning($"Player cannot enter vehicle: {vehicle} (already in a vehicle)", character);
                return;
            }

            var vehiclePublicState = vehicle.GetPublicState<VehiclePublicState>();
            var vehiclePrivateState = vehicle.GetPrivateState<VehiclePrivateState>();
            if (vehiclePublicState.PilotCharacter != null)
            {
                Logger.Warning($"Player cannot enter vehicle: {vehicle} (already has a pilot)", character);
                return;
            }

            // allow to use vehicle even if there is only min energy - to consume it and release reactor cores
            if (!VehicleEnergySystem.SharedHasEnergyCharge(vehicle, 1))
                                                           //Math.Min(protoVehicle.EnergyUsePerSecondIdle,
                                                           //         protoVehicle.EnergyUsePerSecondMoving)))
            {
                Logger.Info("Not enough energy in vehicle to enter it: " + vehicle, character);

                VehicleEnergyConsumptionSystem.ServerNotifyClientNotEnoughEnergy(character, protoVehicle);
                return;
            }

            var characterPrivateState = PlayerCharacter.GetPrivateState(character);
            characterPrivateState.CurrentActionState?.Cancel();

            var currentCharacterInteractionObject = InteractionCheckerSystem.SharedGetCurrentInteraction(character);
            if (currentCharacterInteractionObject != null)
            {
                InteractionCheckerSystem.SharedUnregister(character, currentCharacterInteractionObject, isAbort: true);
            }

            characterPublicState.ServerSetCurrentVehicle(vehicle);

            ServerResetLastVehicleMapMark(vehiclePrivateState);
            vehiclePublicState.PilotCharacter = character;

            vehiclePrivateState.ServerLastPilotCharacter = character;
            characterPrivateState.LastDismountedVehicleMapMark = default;

            Logger.Important("Player entered vehicle: " + vehicle, character);

            // rebuild physics
            character.ProtoWorldObject.SharedCreatePhysics(character);
            vehicle.ProtoWorldObject.SharedCreatePhysics(vehicle);
            Server.World.SetPosition(character, vehicle.Position, writeToLog: false);

            protoVehicle.ServerOnCharacterEnterVehicle(vehicle, character);

            PlayerCharacter.SharedForceRefreshCurrentItem(character);
        }

        private void ServerRemote_ExitVehicle(IDynamicWorldObject gameObjectVehicle)
        {
            var character = ServerRemoteContext.Character;
            ServerCharacterExitVehicle(character, gameObjectVehicle);
        }
    }
}