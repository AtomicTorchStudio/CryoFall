namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.CraftingStations;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class QuestCraftRangedWeapon : ProtoQuest
    {
        public const string CraftMusketOrFlintlockPistol = "Craft musket or flintlock pistol";

        public const string CraftPaperCartridge = "Craft paper cartridge";

        public const string FireTheWeapon = "Fire the weapon";

        public override string Description =>
            "Smashing skulls in with a mace is fine, but sometimes you want something with range. Especially if you go against more serious adversaries.";

        public override string Hints =>
            @"[*] You can unlock all relevant technologies in the ""Offense"" technology group.
              [*] To craft paper cartridges, you will also have to craft [b]gunpowder[/b] and [b]paper[/b] first.
              [*] There are [b]different damage types[/b], and different armor offers different protection values for each. You can see this information in your inventory screen.";

        public override string Name => "Craft primitive firearms";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            var recipeMusket = Api.GetProtoEntity<RecipeMusket>();
            var recipeFlintlockPistol = Api.GetProtoEntity<RecipeFlintlockPistol>();

            tasks
                .Add(TaskBuildStructure.Require<ObjectWeaponWorkbench>())
                .Add(TaskCraftRecipe.RequireStationRecipe<RecipeAmmoPaperCartridge>(
                         description: CraftPaperCartridge))
                .Add(TaskCraftRecipe.RequireStationRecipe(
                                        new List<Recipe.RecipeForStationCrafting>()
                                        {
                                            recipeMusket,
                                            recipeFlintlockPistol
                                        },
                                        description: CraftMusketOrFlintlockPistol)
                                    .WithIcon(ClientItemIconHelper.CreateComposedIcon(this.ShortId,
                                                                                      recipeFlintlockPistol.Icon,
                                                                                      recipeMusket.Icon)))
                .Add(TaskUseItem.Require<IProtoItemWeaponRanged>(
                         description: FireTheWeapon));

            prerequisites
                .Add<QuestUseCrowbarAndDeconstructBuilding>();
        }
    }
}