namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;

    public class QuestUnlockSkills : ProtoQuest
    {
        public const string TaskUnlockAnyCharacterSkills = "Unlock any skill";

        public override string Description =>
            "Each new skill in CryoFall is unlocked as you spend time performing particular activities in a given field. Unlocking more and more skills is a good long-term strategy, as they improve your proficiency in their respective fields.";

        public override string Hints =>
            "[*] High skill levels not only improve your general proficiency in the field, but also offer special perks not accessible otherwise.";

        public override string Name => "Learn some skills";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage2;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskHaveSkills.RequireAny(count: 4,
                                                      minLevel: 1,
                                                      description: TaskUnlockAnyCharacterSkills));

            prerequisites
                .Add<QuestCollectHerbsAndCraftMedicine>();
        }
    }
}