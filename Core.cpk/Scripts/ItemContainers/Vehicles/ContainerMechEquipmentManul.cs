namespace AtomicTorch.CBND.CoreMod.ItemContainers.Vehicles
{
    using AtomicTorch.CBND.CoreMod.Vehicles;

    public class ContainerMechEquipmentManul : BaseItemsContainerMechEquipment
    {
        public override byte AmmoSlotsCount => 3;

        public override VehicleWeaponHardpoint WeaponHardpointName => VehicleWeaponHardpoint.Normal;

        public override byte WeaponSlotsCount => 1;
    }
}