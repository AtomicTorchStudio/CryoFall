namespace AtomicTorch.CBND.CoreMod.ItemContainers.Vehicles
{
    using AtomicTorch.CBND.CoreMod.Vehicles;

    public class ContainerMechEquipmentNemesis : BaseItemsContainerMechEquipment
    {
        public override byte AmmoSlotsCount => 3;

        public override VehicleWeaponHardpoint WeaponHardpointName => VehicleWeaponHardpoint.Exotic;

        public override byte WeaponSlotsCount => 1;
    }
}