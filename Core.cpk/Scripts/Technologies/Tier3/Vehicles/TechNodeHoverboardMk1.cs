namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Vehicles
{
    using AtomicTorch.CBND.CoreMod.Vehicles;

    public class TechNodeHoverboardMk1 : TechNode<TechGroupVehiclesT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddVehicle<VehicleHoverboardMk1>();

            config.SetRequiredNode<TechNodeVehicleAssemblyBay>();
        }
    }
}