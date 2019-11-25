namespace AtomicTorch.CBND.CoreMod.Systems.VehicleGarageSystem
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;

    public enum VehicleStatus : byte
    {
        [Description(CoreStrings.VehicleGarage_VehicleStatus_InWorld)]
        InWorld = 0,

        [Description(CoreStrings.VehicleGarage_VehicleStatus_InGarage)]
        InGarage = 1,

        [Description(CoreStrings.VehicleGarage_VehicleStatus_Docked)]
        Docked = 2,

        [Description(CoreStrings.VehicleGarage_VehicleStatus_InUse)]
        InUse = 3,
    }
}