namespace AtomicTorch.CBND.CoreMod.Vehicles
{
    using System.ComponentModel;

    public enum VehicleCanRepairCheckResult : byte
    {
        Success = 0,

        [Description("Please interact with vehicle to repair it.")]
        NotInteractingWithVehicle = 1,

        // should be never displayed unless it's a hacking attempt
        TechIsNotResearched = 2,

        [Description("Not enough items.")]
        RequiredItemsMissing = 3,

        [Description("Repair is not required.")]
        RepairIsNotRequired = 4,

        [Description("Please bring vehicle to vehicle assembly bay to repair it.")]
        VehicleIsNotInsideVehicleAssemblyBay = 5,

        // re-use description string from SetPowerModeResult
        NotEnoughPower = 6,

        BaseUnderRaidblock = 7
    }
}