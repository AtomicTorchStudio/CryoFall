namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Vehicles
{
    using AtomicTorch.CBND.CoreMod.Vehicles;

    public class TechNodeBehemoth : TechNode<TechGroupVehiclesT5>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddVehicle<VehicleMechBehemoth>();
        }
    }
}