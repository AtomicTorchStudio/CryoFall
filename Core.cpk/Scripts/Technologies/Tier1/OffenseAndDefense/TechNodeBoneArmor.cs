namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.OffenseAndDefense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeBoneArmor : TechNode<TechGroupOffenseAndDefense>
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