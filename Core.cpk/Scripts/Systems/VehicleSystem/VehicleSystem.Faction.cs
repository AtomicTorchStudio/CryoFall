namespace AtomicTorch.CBND.CoreMod.Systems.VehicleSystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public partial class VehicleSystem
    {
        public static void ClientTransferToFactionOwnership(IDynamicWorldObject vehicle)
        {
            Api.Assert(FactionSystem.ClientCurrentFaction is not null, "Player has no faction");
            Api.Assert(string.IsNullOrEmpty(vehicle.GetPublicState<VehiclePublicState>().ClanTag),
                       "Vehicle is already in faction ownership");

            Instance.CallServer(_ => _.ServerRemote_TransferToFactionOwnership(vehicle));
        }

        private static void ServerFactionDissolvedHandler(string clanTag)
        {
            foreach (var vehicle in Server.World.GetWorldObjectsOfProto<IProtoVehicle>())
            {
                var publicState = vehicle.GetPublicState<VehiclePublicState>();
                if (publicState.ClanTag != clanTag)
                {
                    continue;
                }

                publicState.ClanTag = null;
                Logger.Important("Faction-owned vehicle returned to non-faction ownership: " + vehicle);

                WorldObjectOwnersSystem.ServerOnOwnersChanged(vehicle);
            }
        }

        [RemoteCallSettings(timeInterval: 0.5)]
        private void ServerRemote_TransferToFactionOwnership(IDynamicWorldObject vehicle)
        {
            var character = ServerRemoteContext.Character;
            if (!(vehicle.ProtoGameObject is IProtoVehicle))
            {
                throw new Exception("Not a vehicle");
            }

            if (!vehicle.ProtoWorldObject.SharedCanInteract(character,
                                                            vehicle,
                                                            writeToLog: true))
            {
                return;
            }

            var publicState = vehicle.GetPublicState<VehiclePublicState>();
            if (!string.IsNullOrEmpty(publicState.ClanTag))
            {
                Logger.Warning("Already in faction ownership: " + vehicle, character);
                return;
            }

            var faction = FactionSystem.ServerGetFaction(character);
            if (faction is null)
            {
                throw new Exception("Player has no faction");
            }

            if (FactionSystem.SharedGetFactionKind(faction)
                == FactionKind.Public)
            {
                throw new Exception("Cannot transfer a vehicle to ownership of a public faction");
            }

            var clanTag = FactionSystem.SharedGetClanTag(faction);
            publicState.ClanTag = clanTag;

            var privateState = vehicle.GetPrivateState<VehiclePrivateState>();
            //privateState.Owners.Clear(); // keep the original owners list in case the faction is dissolved
            privateState.FactionAccessMode = WorldObjectFactionAccessModes.AllFactionMembers;
            Logger.Important($"Vehicle transferred to faction ownership: {vehicle} - [{clanTag}]", character);

            WorldObjectOwnersSystem.ServerOnOwnersChanged(vehicle);
        }
    }
}