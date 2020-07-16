namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Technologies.Tier1.Construction;

    public class QuestBuildAPermanentBase : ProtoQuest
    {
        public const string HintStructureRelocation =
            "To [b]relocate[/b] an existing structure—select a [b]toolbox[/b] and use it on the structure. Please note that some structures are unmovable.";

        public override string Description =>
            @"Understanding how to protect your base is very important! If you don't do it right, you can be easily raided by more cunning survivors.
              [br]
              [br]The most important point is correctly using the land claim to ensure your base is well protected. To better plan the construction of your base, you should place the land claim structure first, before you place any walls, doors or other structures.
              [br]
              [br]Once your land claim is placed in a good position, you can start constructing walls around it. It is critical to put walls [b][u]inside[/u][/b] of the land claim protection area; otherwise any other survivor would be able to [b]deconstruct them in seconds[/b].
              [br]
              [br]The land claim area can also be expanded to protect more ground, and you can thicken your wall with additional layers to make sure you are well protected. Generally two layers are enough to dissuade most attackers to leave your base alone, unless you have something particularly valuable there.
              [br]
              [br]It is advised to have at least two doors into your base, but having more is even better.";

        public override string Hints =>
            @"[*] Open your land claim menu by interacting with the land claim structure. You can see detailed information about it or upgrade it to higher tiers.
              [*] Structures outside of the land claim area will decay quickly over the course of a day or two. Make sure to build only inside of your land claim area.
              [*] You can deconstruct any structures quickly and conveniently by using a crowbar (from Tier 1 Construction) as long as it's inside your land claim area.
              [*] You don't have to place all crafting stations and other structures inside your base. Since there is nothing to steal, other players will generally leave them be, so you can save a lot of space inside your base for more important structures, such as chests and smelters.";

        public override string Name => "Build a permanent base";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskHaveTechNode.Require<TechNodeLandClaimT1>())
                .Add(TaskBuildStructure.Require<ObjectLandClaimT1>());
            // TODO: restore this for A28 as it will include the necessary localization of the task name
                //.Add(TaskRelocateAnyStructure.Require());

            prerequisites
                .Add<QuestCraftAndEquipClothArmor>()
                .Add<QuestPerformBasicActions>();

            hints
                .Add(HintStructureRelocation);
        }
    }
}