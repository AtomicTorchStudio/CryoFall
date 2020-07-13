namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Vehicles
{
    using AtomicTorch.CBND.CoreMod.Vehicles;

    public class TechNodeHoverboardMk2 : TechNode<TechGroupVehiclesT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddVehicle<VehicleHoverboardMk2>();

            //config.SetRequiredNode<TechNodeHoverboardMk1>();
        }
    }
}