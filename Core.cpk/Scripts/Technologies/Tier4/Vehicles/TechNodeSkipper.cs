namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Vehicles
{
    using AtomicTorch.CBND.CoreMod.Vehicles;

    public class TechNodeSkipper : TechNode<TechGroupVehiclesT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddVehicle<VehicleMechSkipper>();

            config.SetRequiredNode<TechNodeUniversalActuator>();
        }
    }
}