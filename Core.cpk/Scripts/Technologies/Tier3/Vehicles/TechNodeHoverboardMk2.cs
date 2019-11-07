namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Vehicles
{
    using AtomicTorch.CBND.CoreMod.Vehicles;

    public class TechNodeHoverboardMk2 : TechNode<TechGroupVehicles>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddVehicle<VehicleHoverboardMk2>();

            config.SetRequiredNode<TechNodeHoverboardMk1>();
        }
    }
}