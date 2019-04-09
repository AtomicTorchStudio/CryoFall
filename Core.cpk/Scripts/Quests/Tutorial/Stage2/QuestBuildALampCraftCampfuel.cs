namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Industry;

    public class QuestBuildALampCraftCampfuel : ProtoQuest
    {
        public override string Description =>
            "Torches are fine when you are just starting out, but you certainly want to upgrade to something that gives a higher light radius and is more convenient to use. Time to build a lantern and a floor lamp!";

        public override string Hints =>
            @"[*] Lanterns offer a much [b]higher light radius[/b] and more stable light compared to torches.
              [*] Both lanterns and floor lamps use camp fuel to produce light and [b]can be refilled many times[/b], unlike torches, which just burn out.
              [*] There are two ways to produce camp fuel (also known as lamp oil)—either from animal fat or from petroleum products.";

        public override string Name => "Light sources";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementHaveTechNode.Require<TechNodeFloorLampOil>())
                .Add(RequirementHaveTechNode.Require<TechNodeOilLamp>())
                .Add(RequirementBuildStructure.Require<ObjectLightFloorLampOil>())
                .Add(RequirementCraftRecipe.RequireStationRecipe<RecipeOilLamp>())
                .Add(RequirementManufactureItem.Require<ItemCampFuel>(count: 3));

            prerequisites
                .Add<QuestBuildEvaporativeFridge>();
        }
    }
}