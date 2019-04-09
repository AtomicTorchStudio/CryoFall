namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Defense4
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAssaultHeadgear : TechNode<TechGroupDefense4>
    {
        public override string Name => "Assault headgear";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddRecipe<RecipeAssaultHelmet>();

            config.SetRequiredNode<TechNodeAssaultArmor>();
        }
    }
}