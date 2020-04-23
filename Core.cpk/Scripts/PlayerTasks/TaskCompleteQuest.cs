namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Quests;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskCompleteQuest : BasePlayerTaskWithDefaultState
    {
        private TaskCompleteQuest(IProtoQuest quest)
            : base(null)
        {
            this.Quest = quest;
        }

        public override bool IsReversible => true;

        public IProtoQuest Quest { get; }

        // no localization necessary here
        protected override string AutoDescription
            => $"Finish quest: {this.Quest.Name}";

        public static TaskCompleteQuest Require<TQuest>()
            where TQuest : IProtoQuest, new()
        {
            var quest = Api.GetProtoEntity<TQuest>();
            return Require(quest);
        }

        public static TaskCompleteQuest Require(IProtoQuest quest)
        {
            return new TaskCompleteQuest(quest);
        }

        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskState state)
        {
            return character.SharedGetQuests()
                            .SharedHasCompletedQuest(this.Quest);
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                QuestsSystem.ServerCharacterQuestCompleted += this.ServerCharacterQuestCompletedHandler;
            }
            else
            {
                QuestsSystem.ServerCharacterQuestCompleted -= this.ServerCharacterQuestCompletedHandler;
            }
        }

        private void ServerCharacterQuestCompletedHandler(ICharacter character, IProtoQuest quest)
        {
            if (quest != this.Quest)
            {
                return;
            }

            var context = this.GetActiveContext(character, out _);
            context?.Refresh();
        }
    }
}