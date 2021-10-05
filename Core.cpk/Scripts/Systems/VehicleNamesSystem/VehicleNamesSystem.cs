namespace AtomicTorch.CBND.CoreMod.Systems.VehicleNamesSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.ProfanityFiltering;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleGarageSystem;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class VehicleNamesSystem : ProtoSystem<VehicleNamesSystem>
    {
        public const int NameLengthMax = 24;

        public const int NameLengthMin = 3;

        private Dictionary<uint, string> sharedVehicleNames = new();

        public delegate void ClientVehicleNameChangedDelegate(uint vehicleId, string vehicleName);

        public static event ClientVehicleNameChangedDelegate ClientVehicleNameChanged;

        public override string Name => "Vehicle names system";

        public static void ClientSetVehicleName(IDynamicWorldObject vehicle, string vehicleName)
        {
            try
            {
                SharedSanitizeAndValidateName(ref vehicleName);
            }
            catch
            {
                DialogWindow.ShowMessage(
                    title: null,
                    text: string.Format(CoreStrings.VehicleNameEditor_NameRequirements_Format,
                                        NameLengthMin,
                                        NameLengthMax),
                    closeByEscapeKey: true);
                return;
            }

            if (vehicleName != ProfanityFilteringSystem.SharedApplyFilters(vehicleName)
                || vehicleName != Client.SteamApi.FilterText(vehicleName))
            {
                Logger.Warning("Profanity detected: " + vehicleName);
                return;
            }

            var vehicleId = vehicle.Id;
            if (!Instance.SharedIsVehicleNameChangeRequired(vehicleId, vehicleName))
            {
                return;
            }

            Instance.CallServer(_ => _.ServerRemote_SetVehicleName(vehicle, vehicleName));

            Instance.SharedSetVehicleNameInternalNoValidation(vehicleId, vehicleName);

            Logger.Important(
                string.Format("Vehicle name changed: {0} - {1}",
                              vehicleId,
                              string.IsNullOrEmpty(vehicleName)
                                  ? "<no name>"
                                  : vehicleName));

            Api.SafeInvoke(
                () => ClientVehicleNameChanged?.Invoke(vehicleId, vehicleName));
        }

        public static string ClientTryGetVehicleName(uint vehicleId)
        {
            return Instance.sharedVehicleNames.TryGetValue(vehicleId, out var name)
                       ? name
                       : null;
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            Server.Characters.PlayerOnlineStateChanged += this.ServerPlayerOnlineStateChangedHandler;
            WorldObjectOwnersSystem.ServerOwnersChanged += this.ServerWorldObjectsOwnersChangedHandler;
            FactionSystem.ServerCharacterJoinedOrLeftFaction += this.ServerFactionCharacterJoinedOrLeftFactionHandler;

            var storagePrefix = nameof(VehicleNamesSystem);
            var storageKey = "VehicleNames";

            if (Server.Database.TryGet(storagePrefix,
                                       storageKey,
                                       out this.sharedVehicleNames))
            {
                return;
            }

            this.sharedVehicleNames = new Dictionary<uint, string>();
            Server.Database.Set(storagePrefix,
                                storageKey,
                                this.sharedVehicleNames);
        }

        private static void SharedSanitizeAndValidateName(ref string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = null;
                return;
            }

            name = name.Trim();
            if (string.IsNullOrEmpty(name))
            {
                name = null;
                return;
            }

            if (name.Length is < NameLengthMin or > NameLengthMax)
            {
                throw new Exception("Vehicle custom name length exceeded");
            }
        }

        private void ClientRemote_VehicleName(uint vehicleId, string vehicleName)
        {
            this.SharedSetVehicleNameInternalNoValidation(vehicleId, vehicleName);

            Logger.Important(
                string.Format("Vehicle name received: {0} - {1}",
                              vehicleId,
                              string.IsNullOrEmpty(vehicleName)
                                  ? "<no name>"
                                  : vehicleName));

            Api.SafeInvoke(
                () => ClientVehicleNameChanged?.Invoke(vehicleId, vehicleName));
        }

        private void ClientRemote_VehicleNamesList((uint, string)[] vehicleNames)
        {
            this.ClientServerVehicleNames(vehicleNames);
        }

        private void ClientServerVehicleNames((uint, string)[] vehicleNames)
        {
            this.sharedVehicleNames = vehicleNames.ToDictionary(p => p.Item1,
                                                                p => p.Item2);

            Logger.Important(
                "Vehicle names set: "
                + (this.sharedVehicleNames.Count == 0
                       ? "<empty list>"
                       : (Environment.NewLine
                          + this.sharedVehicleNames
                                .Select(p => "* " + p.Key + " - " + p.Value)
                                .GetJoinedString(Environment.NewLine))));
            Api.SafeInvoke(
                () =>
                {
                    var handler = ClientVehicleNameChanged;
                    if (handler is null)
                    {
                        return;
                    }

                    foreach (var pair in this.sharedVehicleNames)
                    {
                        handler.Invoke(vehicleId: pair.Key,
                                       vehicleName: pair.Value);
                    }
                });
        }

        private void ServerFactionCharacterJoinedOrLeftFactionHandler(
            ICharacter character,
            ILogicObject faction,
            bool isJoined)
        {
            // player may have gained or lost access to vehicles
            this.ServerSendOwnedVehicleNames(character);
        }

        private (uint, string)[] ServerGetVehicleNamesKnownForCharacter(
            ICharacter character)
        {
            var vehicles = VehicleGarageSystem.ServerGetCharacterVehicles(character,
                                                                          onlyVehiclesInGarage: false);
            if (vehicles.Count == 0)
            {
                return Array.Empty<(uint, string)>();
            }

            using var result = Api.Shared.GetTempList<(uint, string)>();
            foreach (var vehicle in vehicles)
            {
                var vehicleId = vehicle.Id;
                if (this.sharedVehicleNames.TryGetValue(vehicleId, out var customName))
                {
                    result.Add((vehicleId, customName));
                }
            }

            return result.Count == 0
                       ? Array.Empty<(uint, string)>()
                       : result.AsList().ToArray();
        }

        private void ServerNotifyVehicleOwnersAboutItsCurrentName(IDynamicWorldObject vehicle)
        {
            if (!this.sharedVehicleNames.TryGetValue(vehicle.Id, out var vehicleName))
            {
                return;
            }

            var owners = WorldObjectOwnersSystem.SharedGetOwners(vehicle, isFactionAccess: out _);
            if (owners.Count == 0)
            {
                return;
            }

            // notify all online owners of this vehicle about the current vehicle name change 
            var allOnlinePlayers = Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true);
            foreach (var onlinePlayer in allOnlinePlayers)
            {
                var isOwner = owners.Contains(onlinePlayer.Name);
                if (!isOwner)
                {
                    continue;
                }

                Instance.CallClient(onlinePlayer,
                                    _ => _.ClientRemote_VehicleName(vehicle.Id, vehicleName));
            }
        }

        private void ServerPlayerOnlineStateChangedHandler(ICharacter character, bool isOnline)
        {
            if (isOnline)
            {
                this.ServerSendOwnedVehicleNames(character);
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 2.0, keyArgIndex: 0)]
        private void ServerRemote_SetVehicleName(IDynamicWorldObject vehicle, string vehicleName)
        {
            var character = ServerRemoteContext.Character;
            if (!vehicle.ProtoWorldObject
                        .SharedCanInteract(character, vehicle, writeToLog: true))
            {
                throw new Exception("Cannot interact with " + vehicle);
            }

            if (!WorldObjectOwnersSystem.SharedIsOwner(character, vehicle))
            {
                throw new Exception("Not a vehicle owner: " + vehicle);
            }

            SharedSanitizeAndValidateName(ref vehicleName);

            if (string.IsNullOrEmpty(vehicleName))
            {
                Logger.Warning("Incorrect vehicle name: " + vehicleName);
                return;
            }

            if (vehicleName != ProfanityFilteringSystem.SharedApplyFilters(vehicleName))
            {
                Logger.Warning("Profanity detected: " + vehicleName);
                return;
            }

            var vehicleId = vehicle.Id;
            if (!this.SharedIsVehicleNameChangeRequired(vehicleId, vehicleName))
            {
                return;
            }

            this.SharedSetVehicleNameInternalNoValidation(vehicleId, vehicleName);

            Logger.Important(
                string.Format("Vehicle name changed: {0} - {1}",
                              vehicle,
                              string.IsNullOrEmpty(vehicleName)
                                  ? "<no name>"
                                  : vehicleName),
                character);

            // notify all online owners of this vehicle about the vehicle name change 
            var owners = WorldObjectOwnersSystem.SharedGetOwners(vehicle, isFactionAccess: out _);
            var allOnlinePlayers = Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true);
            foreach (var onlinePlayer in allOnlinePlayers)
            {
                var isOwner = owners.Contains(onlinePlayer.Name);
                if (!isOwner
                    || onlinePlayer == character)
                {
                    // not an owner or the same player that has just assigned the new vehicle name
                    continue;
                }

                Instance.CallClient(onlinePlayer,
                                    _ => _.ClientRemote_VehicleName(vehicleId, vehicleName));
            }
        }

        private void ServerSendOwnedVehicleNames(ICharacter character)
        {
            if (!character.ServerIsOnline)
            {
                return;
            }

            var vehicleNames = this.ServerGetVehicleNamesKnownForCharacter(character);
            this.CallClient(character,
                            _ => _.ClientRemote_VehicleNamesList(vehicleNames));
        }

        private void ServerWorldObjectsOwnersChangedHandler(IWorldObject worldObject)
        {
            if (worldObject is IDynamicWorldObject vehicle
                && worldObject.ProtoGameObject is IProtoVehicle)
            {
                this.ServerNotifyVehicleOwnersAboutItsCurrentName(vehicle);
            }
        }

        private bool SharedIsVehicleNameChangeRequired(uint vehicleId, string vehicleName)
        {
            if (this.sharedVehicleNames.TryGetValue(vehicleId, out var existingVehicleName))
            {
                if (existingVehicleName == vehicleName)
                {
                    // player requested to set the custom name which is already set
                    return false;
                }
            }
            else if (string.IsNullOrEmpty(vehicleName))
            {
                // player requested to reset the custom name but vehicle doesn't have it
                return false;
            }

            return true;
        }

        private void SharedSetVehicleNameInternalNoValidation(uint vehicleId, string vehicleName)
        {
            if (string.IsNullOrEmpty(vehicleName))
            {
                this.sharedVehicleNames.Remove(vehicleId);
            }
            else
            {
                this.sharedVehicleNames[vehicleId] = vehicleName;
            }
        }
    }
}