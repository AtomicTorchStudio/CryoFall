namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;

    public class QuestBuildMedicalLab : ProtoQuest
    {
        public override string Description =>
            "Medical science gives access to a variety of useful effects and possibilities, not just the ability to heal injuries and diseases.";

        public override string Hints =>
            "[*] Higher tiers of medical technology give access to more powerful healing items and other meds.";

        public override string Name => "Build medical lab";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementBuildStructure.Require<ObjectMedicalLab>())
                .Add(RequirementCraftRecipe.RequireStationRecipe<RecipeBandage>())
                .Add(RequirementCraftRecipe.RequireStationRecipe<RecipeStrengthBoostSmall>())
                .Add(RequirementUseItem.Require<ItemStrengthBoostSmall>());

            prerequisites
                .Add<QuestCompleteTier1Technologies>();
        }
    }
}