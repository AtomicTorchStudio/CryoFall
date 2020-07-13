namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Vehicles
{
    using AtomicTorch.CBND.CoreMod.Vehicles;

    public class TechNodeManul : TechNode<TechGroupVehiclesT4>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddVehicle<VehicleMechManul>();

            config.SetRequiredNode<TechNodeSkipper>();
        }
    }
}