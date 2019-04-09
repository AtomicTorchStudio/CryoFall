namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;

    public class QuestCraftAKnifeAndKillAnyCreature : ProtoQuest
    {
        public const string TaskKillAnyCreature = "Kill any creature";

        public const string TaskLootAnyCreature = "Loot any creature";

        public override string Description =>
            "Knives are a good early weapon to use against less dangerous beasts. It is always good to have one before you can craft something more substantial.";

        public override string Hints =>
            "[*] Each weapon in CryoFall has some [b]special properties[/b]. For example, knives have a higher chance to cause bleeding, while blunt weapons may break bones or daze the enemy.";

        public override string Name => "Craft a knife and kill any creature";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementCraftRecipe.RequireHandRecipe<RecipeKnifeStone>())
                .Add(RequirementKill.Require<IProtoCharacterMob>(count: 1, description: TaskKillAnyCreature))
                .Add(RequirementGather.Require<ObjectCorpse>(count: 1, description: TaskLootAnyCreature));

            prerequisites
                .Add<QuestUnlockAndBuildWorkbench>();
        }
    }
}