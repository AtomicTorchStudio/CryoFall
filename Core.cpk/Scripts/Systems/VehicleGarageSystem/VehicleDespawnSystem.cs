namespace AtomicTorch.CBND.CoreMod.Systems.VehicleGarageSystem
{
    using System;
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
        public const double OfflineDurationToDespawn = CharacterDespawnSystem.OfflineDurationToDespawn;

        [NotLocalizable]
        public override string Name => "Vehicle despawn system";

        public static bool ServerIsVehicleInsideOwnerBase(IDynamicWorldObject vehicle)
        {
            var vehicleCurrentBase = LandClaimSystem.SharedGetLandClaimAreasGroup(vehicle.TilePosition);
            if (vehicleCurrentBase is null)
            {
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
                        return true;
                    }
                }
            }

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

            TriggerTimeInterval.ServerConfigureAndRegister(
                callback: ServerUpdate,
                name: "System." + this.ShortId,
                interval: TimeSpan.FromSeconds(10));
        }

        private static bool ServerIsNeedDespawn(IDynamicWorldObject vehicle)
        {
            var privateState = vehicle.GetPrivateState<VehiclePrivateState>();
            return !privateState.IsInGarage
                   && privateState.ServerTimeSinceLastUse > OfflineDurationToDespawn
                   && !ServerIsVehicleInsideOwnerBase(vehicle);
        }

        private static void ServerUpdate()
        {
            var allVehicles = Server.World.GetWorldObjectsOfProto<IProtoVehicle>();
            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (IDynamicWorldObject vehicle in allVehicles)
            {
                if (ServerIsNeedDespawn(vehicle))
                {
                    VehicleGarageSystem.ServerPutIntoGarage(vehicle);
                }
            }
        }
    }
}