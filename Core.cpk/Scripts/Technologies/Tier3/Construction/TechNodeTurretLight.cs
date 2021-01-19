namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Construction
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses;

    public class TechNodeTurretLight : TechNode<TechGroupConstructionT3>
    {
        public override FeatureAvailability AvailableIn => FeatureAvailability.OnlyPvP;

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectTurretLight>();

            config.SetRequiredNode<TechNodeWallFence>();
        }
    }
}