namespace AtomicTorch.CBND.CoreMod.Technologies.Tier4.Defense
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;

    public class TechNodeAssaultHeadgear : TechNode<TechGroupDefenseT4>
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