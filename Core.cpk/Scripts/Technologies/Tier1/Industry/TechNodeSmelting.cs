﻿namespace AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;

    public class TechNodeSmelting : TechNode<TechGroupIndustryT1>
    {
        public override string Name => "Smelting";

        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectFurnace>()
                  .AddRecipe<RecipeIngotCopperFromOre>()
                  .AddRecipe<RecipeIngotCopperFromConcentrate>(isHidden: true)
                  .AddRecipe<RecipeIngotIronFromOre>()
                  .AddRecipe<RecipeIngotIronFromConcentrate>(isHidden: true);

            config.SetRequiredNode<TechNodeWorkbench>();
        }
    }
}