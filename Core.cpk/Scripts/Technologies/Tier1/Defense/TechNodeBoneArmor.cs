namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Defense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeBoneArmor : TechNode<TechGroupDefense>
    {
        public override string Name => "Bone armor";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeBoneJacket>()
                  .AddRecipe<RecipeBoneHelmetHorned>()
                  .AddRecipe<RecipeBoneHelmetClosed>()
                  .AddRecipe<RecipeBonePants>();

            config.SetRequiredNode<TechNodeWoodArmor>();
        }
    }
}