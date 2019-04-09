namespace AtomicTorch.CBND.CoreMod.Quests
{
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public interface IQuestRequirement
    {
        string Description { get; }

        bool IsReversible { get; }

        IProtoQuest Quest { get; set; }

        QuestRequirementState CreateState();

        bool IsStateTypeMatch(QuestRequirementState state);

        void ServerRefreshIsSatisfied(ICharacter character, QuestRequirementState state);

        void ServerRegisterOrUnregisterContext(ServerCharacterActiveQuestRequirement context);
    }
}