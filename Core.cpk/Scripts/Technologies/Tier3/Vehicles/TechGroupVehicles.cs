using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Chemistry;
using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Construction2;
using AtomicTorch.CBND.CoreMod.Technologies.Tier2.Industry2;

namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Vehicles
{

    public class TechGroupVehicles : TechGroup
    {
        public override string Description => "Technologies necessary to produce a variety of different vehicles.";

        public override string Name => "Vehicles";

        public override TechTier Tier => TechTier.Tier3;

        protected override void PrepareTechGroup(Requirements requirements)
        {
            requirements.AddGroup<TechGroupIndustry2>(completion: 1);
            requirements.AddGroup<TechGroupConstruction2>(completion: 1);
            requirements.AddGroup<TechGroupChemistry>(completion: 1);
        }
    }
}