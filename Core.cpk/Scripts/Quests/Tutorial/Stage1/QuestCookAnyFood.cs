namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class QuestCookAnyFood : ProtoQuest
    {
        public const string TaskEatAnyFood = "Eat any food";

        public override string Description =>
            "Now that you've built a campfire, you can use it to cook some basic food. If you have mushrooms or meat, you can cook them to make them more nutritious and safe to eat.";

        public override string Hints =>
            @"[*] You can see [b]all possible recipes[/b] in a campfire if you press ""Browse recipes"".
              [*] In order to cook something, you need to [b]select an appropriate recipe[/b].
              [*] Campfires require [b]fuel[/b] to work. Place some [b]wood or charcoal[/b] in the fuel slot.
              [*] You can learn [b]new recipes[/b] in the [b]technology menu[/b] later.
              [*] You can use any item (including food) either directly [b]from inventory[/b] or by placing it in the [b]hotbar[/b] and using in-game.";

        public override string Name => "Cook any food";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage1;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks)
        {
            tasks
                .Add(TaskManufactureItem.Require<IProtoItemFood>(count: 1,
                                                                        description: this.Name))
                .Add(TaskUseItem.Require<IProtoItemFood>(count: 1,
                                                                description: TaskEatAnyFood));

            prerequisites
                .Add<QuestBuildACampfire>();
        }
    }
}