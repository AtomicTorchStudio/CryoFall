namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Vehicles2
{
    using AtomicTorch.CBND.CoreMod.Vehicles;

    public class TechNodeSkipper : TechNode<TechGroupVehicles2>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddVehicle<VehicleMechSkipper>();

            config.SetRequiredNode<TechNodeUniversalActuator>();
        }
    }
}