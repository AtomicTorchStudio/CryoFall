namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;

    public class QuestCraftRangedWeapon : ProtoQuest
    {
        public override string Description =>
            "Smashing skulls in with a mace is fine, but sometimes you want something with range. Especially if you go against more serious adversaries.";

        public override string Hints =>
            @"[*] You can unlock all relevant technologies in the ""Offense & Defense"" technology group.
              [*] To craft paper cartridges, you will also have to craft [b]gunpowder[/b] and [b]paper[/b] first.
              [*] There are [b]different damage types[/b], and different armor offers different protection values for each. You can see this information in your inventory screen.";

        public override string Name => "Craft ranged weapon";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementBuildStructure.Require<ObjectWeaponWorkbench>())
                .Add(RequirementCraftRecipe.RequireStationRecipe<RecipeAmmoPaperCartridge>())
                .Add(RequirementCraftRecipe.RequireStationRecipe<RecipeMusket>())
                .Add(RequirementUseItem.Require<ItemMusket>());

            prerequisites
                .Add<QuestBuildFurnaceAndSmeltCopper>();
        }
    }
}