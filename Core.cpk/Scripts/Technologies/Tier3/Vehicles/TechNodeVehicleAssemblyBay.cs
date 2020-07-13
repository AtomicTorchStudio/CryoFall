namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Vehicles
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc;

    public class TechNodeVehicleAssemblyBay : TechNode<TechGroupVehiclesT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectVehicleAssemblyBay>();
        }
    }
}