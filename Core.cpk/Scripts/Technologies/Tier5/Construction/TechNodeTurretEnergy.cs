namespace AtomicTorch.CBND.CoreMod.Technologies.Tier5.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses;

    public class TechNodeTurretEnergy : TechNode<TechGroupConstructionT5>
    {
        public override FeatureAvailability AvailableIn => FeatureAvailability.OnlyPvP;

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectTurretEnergy>();

            config.SetRequiredNode<TechNodeSteelConstructions>();
        }
    }
}