namespace AtomicTorch.CBND.CoreMod.Systems.VehicleGarageSystem
{
    using System.ComponentModel;

    public enum TakeVehicleResult : byte
    {
        Success,

        [Description("Blocked. Assembly bay must be cleared of obstacles and people to place a vehicle.")]
        SpaceBlocked,

        NotOwner,

        Unknown,

        Error_Docked,

        Error_InUse,

        BaseUnderRaidblock
    }
}