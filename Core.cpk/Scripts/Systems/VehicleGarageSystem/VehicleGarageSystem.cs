namespace AtomicTorch.CBND.CoreMod.Systems.VehicleGarageSystem
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier3.Vehicles;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.VehicleAssemblyBay;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class VehicleGarageSystem : ProtoSystem<VehicleGarageSystem>
    {
        public const string Notification_CannotTakeVehicle_Title = "Cannot take vehicle";

        public const string Notification_VehicleInGarage_Description =
            "Your vehicle is in the garage. You can take it from any Vehicle Assembly Bay.";

        public const string Notification_VehiclesInGarage_Description =
            "Some of your vehicles are in the garage. You can take them from any Vehicle Assembly Bay.";

        public const string Notification_VehiclesInGarage_Title = "Vehicles in garage";

        /// <summary>
        /// To prevent other players from taking vehicles (from world into garage) that were used recently
        /// by anyone else, a time delay applies (vehicles considered "in use" for this duration after pilot left).
        /// </summary>
        private const double ThresholdNoPilotSeconds = 5 * 60; // 5 minutes

        public static readonly SoundResource SoundResourcePutVehicle
            = new("Objects/Structures/ObjectVehicleAssemblyBay/PutVehicle");

        public static readonly SoundResource SoundResourceTakeVehicle
            = new("Objects/Structures/ObjectVehicleAssemblyBay/TakeVehicle");

        public override string Name => "Vehicle garage system";

        public static Task<IReadOnlyList<GarageVehicleEntry>> ClientGetVehiclesListAsync()
        {
            return Instance.CallServer(_ => _.ServerRemote_GetVehiclesList());
        }

        public static bool ClientIsVehicleDocked(
            IDynamicWorldObject vehicle,
            IStaticWorldObject vehicleAssemblyBay)
        {
            foreach (var o in Client.World
                                    .GetTile(vehicle.TilePosition)
                                    .StaticObjects)
            {
                if (ReferenceEquals(o, vehicleAssemblyBay))
                {
                    return true;
                }
            }

            return false;
        }

        public static async void ClientPutCurrentVehicle()
        {
            if (!PveSystem.ClientIsPve(logErrorIfDataIsNotYetAvailable: true))
            {
                // this feature is available only in PvE
                return;
            }

            var vehicleAssemblyBay = InteractionCheckerSystem.SharedGetCurrentInteraction(
                ClientCurrentCharacterHelper.Character);
            var isSuccess = await Instance.CallServer(_ => _.ServerRemote_PutCurrentVehicle());
            if (isSuccess)
            {
                Client.Audio.PlayOneShot(SoundResourcePutVehicle, vehicleAssemblyBay);
            }
        }

        public static async void ClientTakeVehicle(uint vehicleGameObjectId)
        {
            var vehicleAssemblyBay = InteractionCheckerSystem.SharedGetCurrentInteraction(
                ClientCurrentCharacterHelper.Character);

            var result = await Instance.CallServer(_ => _.ServerRemote_TakeVehicle(vehicleGameObjectId));
            if (result == TakeVehicleResult.Success)
            {
                Client.Audio.PlayOneShot(SoundResourceTakeVehicle, vehicleAssemblyBay);
                WindowObjectVehicleAssemblyBay.CloseActiveMenu();
                return;
            }

            if (result == TakeVehicleResult.BaseUnderRaidblock)
            {
                LandClaimSystem.SharedSendNotificationActionForbiddenUnderRaidblock(
                    ClientCurrentCharacterHelper.Character);
                return;
            }

            var error = result.GetDescription();
            if (string.IsNullOrEmpty(error))
            {
                return;
            }

            var currentInteractionObject =
                InteractionCheckerSystem.SharedGetCurrentInteraction(ClientCurrentCharacterHelper.Character);
            if (!(currentInteractionObject?.ProtoWorldObject is IProtoVehicleAssemblyBay protoVehicleAssemblyBay))
            {
                return;
            }

            NotificationSystem.ClientShowNotification(
                Notification_CannotTakeVehicle_Title,
                error,
                NotificationColor.Bad,
                protoVehicleAssemblyBay.Icon);
        }

        public static void ServerPutIntoGarage(IDynamicWorldObject vehicle)
        {
            var position = ServerCharacterDeathMechanic.ServerGetGraveyardPosition().ToVector2D();

            var vehiclePrivateState = vehicle.GetPrivateState<VehiclePrivateState>();
            if (vehiclePrivateState.IsInGarage
                && vehicle.Position == position)
            {
                // already in garage
                return;
            }

            var vehicleCurrentPilot = vehicle.GetPublicState<VehiclePublicState>().PilotCharacter;
            if (vehicleCurrentPilot is not null)
            {
                VehicleSystem.ServerCharacterExitCurrentVehicle(vehicleCurrentPilot, force: true);
            }

            vehiclePrivateState.IsInGarage = true;

            Server.World.SetPosition(vehicle,
                                     position,
                                     writeToLog: false);

            VehicleSystem.ServerResetLastVehicleMapMark(vehiclePrivateState);

            vehicle.ProtoWorldObject.SharedCreatePhysics(vehicle);
            Logger.Important("Vehicle put into the garage: " + vehicle,
                             characterRelated: ServerRemoteContext.IsRemoteCall
                                                   ? ServerRemoteContext.Character
                                                   : null);

            if (ServerRemoteContext.IsRemoteCall)
            {
                // this action is done by player
                return;
            }

            // server put vehicle into garage - notify owners
            foreach (var owner in vehiclePrivateState.Owners)
            {
                var player = Server.Characters.GetPlayerCharacter(owner);
                if (player is null)
                {
                    continue;
                }

                var protoVehicle = (IProtoVehicle)vehicle.ProtoGameObject;
                Instance.CallClient(player, _ => _.ClientRemote_VehicleInGarage(protoVehicle));
            }
        }

        private static bool ServerCanCharacterPutVehicleIntoGarage(IDynamicWorldObject vehicle, ICharacter byCharacter)
        {
            if (!PveSystem.ServerIsPvE)
            {
                throw new Exception("This feature is available only in PvE");
            }

            var status = ServerGetVehicleStatus(vehicle, byCharacter);
            switch (status)
            {
                case VehicleStatus.Docked:
                    // Anyone can put a docked vehicle into garage
                    // but it could be taken back only by its owners.
                    // This is necessary to prevent people from bringing vehicles
                    // to another player's base and blocking the vehicle assembly bay.
                    return true;

                case VehicleStatus.InWorld
                    when WorldObjectOwnersSystem.SharedIsOwner(byCharacter, vehicle):
                    return true;

                default:
                    return false;
            }
        }

        private static List<GarageVehicleEntry> ServerGetCharacterVehicles(
            ICharacter character,
            bool onlyVehiclesInGarage)
        {
            var result = new List<GarageVehicleEntry>();
            var allVehicles = Server.World.GetWorldObjectsOfProto<IProtoVehicle>();

            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (IDynamicWorldObject vehicle in allVehicles)
            {
                if (!WorldObjectOwnersSystem.SharedIsOwner(character, vehicle))
                {
                    continue;
                }

                var vehicleStatus = ServerGetVehicleStatus(vehicle, forCharacter: character);
                if (onlyVehiclesInGarage
                    && vehicleStatus != VehicleStatus.InGarage)
                {
                    continue;
                }

                result.Add(new GarageVehicleEntry(vehicle,
                                                  vehicleStatus));
            }

            return result;
        }

        private static VehicleStatus ServerGetVehicleStatus(IDynamicWorldObject vehicle, ICharacter forCharacter)
        {
            var vehiclePrivateState = vehicle.GetPrivateState<VehiclePrivateState>();
            if (vehiclePrivateState.IsInGarage)
            {
                return VehicleStatus.InGarage;
            }

            var publicState = vehicle.GetPublicState<VehiclePublicState>();
            if (publicState.PilotCharacter is not null)
            {
                return VehicleStatus.InUse;
            }

            var insideBay = false;

            foreach (var o in Server.World
                                    .GetTile(vehicle.TilePosition)
                                    .StaticObjects)
            {
                if (!(o.ProtoGameObject is IProtoVehicleAssemblyBay protoVehicleAssemblyBay))
                {
                    continue;
                }

                var tempVehicles = Api.Shared.GetTempList<IDynamicWorldObject>();
                protoVehicleAssemblyBay.SharedGetVehiclesOnPlatform(o, tempVehicles);
                if (tempVehicles.Contains(vehicle))
                {
                    insideBay = true;
                    break;
                }
            }

            if (insideBay)
            {
                return VehicleStatus.Docked;
            }

            if (forCharacter == vehiclePrivateState.ServerLastPilotCharacter)
            {
                return VehicleStatus.InWorld;
            }

            return vehiclePrivateState.ServerTimeSinceLastUse < ThresholdNoPilotSeconds
                       ? VehicleStatus.InUse // was used recently
                       : VehicleStatus.InWorld;
        }

        private void ClientRemote_OnVehiclePutToGarageByOtherPlayer(Vector2D position)
        {
            Client.Audio.PlayOneShot(SoundResourcePutVehicle, position);
        }

        private void ClientRemote_OnVehicleTakenFromGarageByOtherPlayer(Vector2D position)
        {
            Client.Audio.PlayOneShot(SoundResourceTakeVehicle, position);
        }

        // displayed when player logging into the game and has a single vehicle in garage
        // or when the vehicle is despawned to garage
        private void ClientRemote_VehicleInGarage(IProtoVehicle protoVehicle)
        {
            NotificationSystem.ClientShowNotification(protoVehicle.Name,
                                                      Notification_VehicleInGarage_Description,
                                                      icon: protoVehicle.Icon)
                              .HideAfterDelay(delaySeconds: 60);
        }

        // displayed when player logging into the game and has multiple vehicles in garage
        private void ClientRemote_VehiclesInGarage()
        {
            NotificationSystem.ClientShowNotification(Notification_VehiclesInGarage_Title,
                                                      Notification_VehiclesInGarage_Description,
                                                      icon: Api.GetProtoEntity<TechGroupVehiclesT3>().Icon)
                              .HideAfterDelay(delaySeconds: 60);
        }

        [RemoteCallSettings(timeInterval: RemoteCallSettingsAttribute.MaxTimeInterval)]
        private void ServerRemote_CheckHasVehiclesInGarage()
        {
            var character = ServerRemoteContext.Character;
            var vehicles = ServerGetCharacterVehicles(character,
                                                      onlyVehiclesInGarage: true);
            if (vehicles.Count == 0)
            {
                return;
            }

            if (vehicles.Count == 1)
            {
                // player has a single vehicle in garage
                var protoVehicle = vehicles[0].ProtoVehicle;
                this.CallClient(character, _ => _.ClientRemote_VehicleInGarage(protoVehicle));
                return;
            }

            // players has multiple vehicles in garage
            this.CallClient(character, _ => _.ClientRemote_VehiclesInGarage());
        }

        private IReadOnlyList<GarageVehicleEntry> ServerRemote_GetVehiclesList()
        {
            var character = ServerRemoteContext.Character;
            var currentInteractionObject = InteractionCheckerSystem.SharedGetCurrentInteraction(character);
            if (!(currentInteractionObject?.ProtoWorldObject is IProtoVehicleAssemblyBay))
            {
                Logger.Warning(character + " is not interacting with any vehicle assembly bay");
                return Array.Empty<GarageVehicleEntry>();
            }

            var onlyVehiclesInGarage = !PveSystem.ServerIsPvE;
            return ServerGetCharacterVehicles(character, onlyVehiclesInGarage);
        }

        private bool ServerRemote_PutCurrentVehicle()
        {
            if (!PveSystem.ServerIsPvE)
            {
                throw new Exception("This feature is available only in PvE");
            }

            var character = ServerRemoteContext.Character;
            var currentInteractionObject = InteractionCheckerSystem.SharedGetCurrentInteraction(character);
            if (!(currentInteractionObject?.ProtoWorldObject is IProtoVehicleAssemblyBay protoVehicleAssemblyBay))
            {
                throw new Exception("Player is not interacting with an vehicle assembly bay");
            }

            using var tempVehiclesList = Api.Shared.GetTempList<IDynamicWorldObject>();
            protoVehicleAssemblyBay.SharedGetVehiclesOnPlatform(
                vehicleAssemblyBay: (IStaticWorldObject)currentInteractionObject,
                tempVehiclesList);

            var isPutAtLeastOne = false;
            foreach (var vehicle in tempVehiclesList.AsList())
            {
                if (ServerCanCharacterPutVehicleIntoGarage(vehicle, byCharacter: character))
                {
                    ServerPutIntoGarage(vehicle);
                    isPutAtLeastOne = true;
                }
            }

            if (isPutAtLeastOne)
            {
                // notify other players in scope
                var soundPosition = currentInteractionObject.TilePosition.ToVector2D()
                                    + protoVehicleAssemblyBay.PlatformCenterWorldOffset;
                using var tempPlayers = Api.Shared.GetTempList<ICharacter>();
                Server.World.GetScopedByPlayers(currentInteractionObject, tempPlayers);
                tempPlayers.Remove(character);

                Instance.CallClient(tempPlayers.AsList(),
                                    _ => _.ClientRemote_OnVehiclePutToGarageByOtherPlayer(soundPosition));
            }

            return isPutAtLeastOne;
        }

        private TakeVehicleResult ServerRemote_TakeVehicle(uint vehicleGameObjectId)
        {
            var character = ServerRemoteContext.Character;
            var currentInteractionObject = InteractionCheckerSystem.SharedGetCurrentInteraction(character);
            if (!(currentInteractionObject?.ProtoWorldObject is IProtoVehicleAssemblyBay protoVehicleAssemblyBay))
            {
                Logger.Warning("Player is not interacting with an vehicle assembly bay", character);
                return TakeVehicleResult.Unknown;
            }

            var vehicleAssemblyBay = (IStaticWorldObject)currentInteractionObject;
            var vehicle = Server.World.GetGameObjectById<IDynamicWorldObject>(GameObjectType.DynamicObject,
                                                                              vehicleGameObjectId);
            if (vehicle is null)
            {
                Logger.Warning("Vehicle is not found", character);
                return TakeVehicleResult.Unknown;
            }

            if (!WorldObjectOwnersSystem.SharedIsOwner(character, vehicle))
            {
                Logger.Warning("Not an owner of the vehicle: " + vehicle, character);
                return TakeVehicleResult.NotOwner;
            }

            var status = ServerGetVehicleStatus(vehicle, forCharacter: character);
            switch (status)
            {
                case VehicleStatus.InGarage:
                    // allow to take
                    break;

                case VehicleStatus.InWorld:
                    if (!PveSystem.ServerIsPvE)
                    {
                        Logger.Warning("Cannot take a vehicle from world on a PvP server", character);
                        return TakeVehicleResult.Unknown;
                    }

                    // allow to take a vehicle from world in PvE only
                    break;

                case VehicleStatus.InUse:
                    return TakeVehicleResult.Error_InUse;

                case VehicleStatus.Docked:
                    //return TakeVehicleResult.Error_Docked;
                    // allow to take
                    break;

                default:
                    return TakeVehicleResult.Unknown;
            }

            if (IsServer
                && LandClaimSystem.SharedIsUnderRaidBlock(character, vehicleAssemblyBay))
            {
                return TakeVehicleResult.BaseUnderRaidblock;
            }

            if (protoVehicleAssemblyBay.SharedIsBaySpaceBlocked(
                vehicleAssemblyBay: vehicleAssemblyBay))
            {
                return TakeVehicleResult.SpaceBlocked;
            }

            var position = currentInteractionObject.TilePosition.ToVector2D()
                           + protoVehicleAssemblyBay.PlatformCenterWorldOffset;

            var vehiclePrivateState = vehicle.GetPrivateState<VehiclePrivateState>();

            vehiclePrivateState.IsInGarage = false;
            vehiclePrivateState.ServerTimeSincePilotOffline = 0;
            vehiclePrivateState.ServerTimeSinceLastUse = ThresholdNoPilotSeconds + 1;

            Server.World.SetPosition(vehicle,
                                     position,
                                     writeToLog: false);

            vehicle.ProtoWorldObject.SharedCreatePhysics(vehicle);
            Logger.Important("Vehicle taken out of the garage: " + vehicle, character);

            // notify other players in scope
            var soundPosition = currentInteractionObject.TilePosition.ToVector2D()
                                + protoVehicleAssemblyBay.PlatformCenterWorldOffset;
            using var tempPlayers = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(currentInteractionObject, tempPlayers);
            tempPlayers.Remove(character);

            Instance.CallClient(tempPlayers.AsList(),
                                _ => _.ClientRemote_OnVehicleTakenFromGarageByOtherPlayer(soundPosition));

            return TakeVehicleResult.Success;
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += Refresh;

                Refresh();

                void Refresh()
                {
                    if (Api.Client.Characters.CurrentPlayerCharacter is null)
                    {
                        return;
                    }

                    Instance.CallServer(_ => _.ServerRemote_CheckHasVehiclesInGarage());
                }
            }
        }
    }
}