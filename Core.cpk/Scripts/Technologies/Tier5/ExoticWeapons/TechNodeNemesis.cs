namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.ExoticWeapons
{
    using AtomicTorch.CBND.CoreMod.Vehicles;

    public class TechNodeNemesis : TechNode<TechGroupExoticWeaponsT5>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddVehicle<VehicleMechNemesis>();

            config.SetRequiredNode<TechNodeAmmoKeinite>();
        }
    }
}