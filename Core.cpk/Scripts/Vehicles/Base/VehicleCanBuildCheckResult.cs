namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using System.ComponentModel;
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum VehicleCanBuildCheckResult : byte
    {
        Success = 0,

        [Description("Please use a vehicle bay to build this vehicle.")]
        NotInteractingWithVehicleBay = 1,

        // should be never displayed unless it's a hacking attempt
        TechIsNotResearched = 2,

        [Description("Not enough items.")]
        RequiredItemsMissing = 3,

        [Description("Blocked.[br]Assembly bay must be cleared of obstacles and people to construct a schematic.")]
        NeedsFreeSpace = 4,

        // re-use description string from SetPowerModeResult
        NotEnoughPower = 5,

        BaseUnderRaidblock = 6
    }
}