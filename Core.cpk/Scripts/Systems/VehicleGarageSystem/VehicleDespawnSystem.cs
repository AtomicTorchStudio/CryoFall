namespace AtomicTorch.CBND.CoreMod.Systems.VehicleGarageSystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDespawnSystem;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// Vehicles should be placed into garage on PvE servers after some delay.
    /// They could be taken from any garage after that.
    /// </summary>
    public class VehicleDespawnSystem : ProtoSystem<VehicleDespawnSystem>
    {
        private static readonly Lazy<IProtoVehicle[]> LazyAllProtoVehicles
            = new(() => Api.FindProtoEntities<IProtoVehicle>().ToArray());

        [NotLocalizable]
        public override string Name => "Vehicle despawn system";

        public static bool ServerIsVehicleInsideOwnerBase(IDynamicWorldObject vehicle)
        {
            return ServerIsVehicleInsideOwnerBase(vehicle, out _);
        }

        public static bool ServerIsVehicleInsideOwnerBase(
            IDynamicWorldObject vehicle,
            out bool isInsideNotOwnedBase)
        {
            var vehicleCurrentBase = LandClaimSystem.SharedGetLandClaimAreasGroup(vehicle.TilePosition);
            if (vehicleCurrentBase is null)
            {
                isInsideNotOwnedBase = false;
                return false;
            }

            var vehicleOwners = vehicle.GetPrivateState<VehiclePrivateState>().Owners;
            foreach (var area in LandClaimAreasGroup.GetPrivateState(vehicleCurrentBase)
                                                    .ServerLandClaimsAreas)
            {
                using var areaOwners = Api.Shared.WrapInTempList(
                    LandClaimArea.GetPrivateState(area)
                                 .ServerGetLandOwners());

                foreach (var ownerName in vehicleOwners)
                {
                    if (areaOwners.Contains(ownerName))
                    {
                        isInsideNotOwnedBase = false;
                        return true;
                    }
                }
            }

            isInsideNotOwnedBase = true;
            return false;
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            if (!PveSystem.ServerIsPvE)
            {
                return;
            }

            if (SharedLocalServerHelper.IsLocalServer)
            {
                // don't despawn abandoned vehicles on the local server
                return;
            }

            // setup timer (tick every frame)
            TriggerEveryFrame.ServerRegister(
                callback: ServerGlobalUpdate,
                name: "System." + this.ShortId);
        }

        private static void ServerGlobalUpdate()
        {
            // perform update once per 10 seconds per vehicle
            const double spreadDeltaTime = 10;

            using var tempListVehicles = Api.Shared.GetTempList<IDynamicWorldObject>();
            foreach (var protoVehicle in LazyAllProtoVehicles.Value)
            {
                tempListVehicles.Clear();
                protoVehicle.EnumerateGameObjectsWithSpread(tempListVehicles.AsList(),
                                                            spreadDeltaTime: spreadDeltaTime,
                                                            Server.Game.FrameNumber,
                                                            Server.Game.FrameRate);
                foreach (var vehicle in tempListVehicles.AsList())
                {
                    if (ServerIsNeedDespawn(vehicle))
                    {
                        VehicleGarageSystem.ServerPutIntoGarage(vehicle);
                    }
                }
            }
        }

        private static bool ServerIsNeedDespawn(IDynamicWorldObject vehicle)
        {
            var privateState = vehicle.GetPrivateState<VehiclePrivateState>();
            if (privateState.IsInGarage)
            {
                return false;
            }

            var publicState = vehicle.GetPublicState<VehiclePublicState>();
            if (publicState.PilotCharacter is not null)
            {
                return false;
            }

            if (ServerIsVehicleInsideOwnerBase(vehicle, out var isInsideNotOwnedBase))
            {
                return false;
            }

            var durationToDespawn = isInsideNotOwnedBase
                                        ? CharacterDespawnSystem.OfflineDurationToDespawnWhenInsideNotOwnedClaim
                                        : CharacterDespawnSystem.OfflineDurationToDespawnWhenInFreeLand;
            if (privateState.ServerTimeSinceLastUse < durationToDespawn)
            {
                return false;
            }

            return true;
        }
    }
}