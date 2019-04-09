namespace AtomicTorch.CBND.CoreMod.Quests
{
    public abstract class QuestRequirementWithDefaultState : QuestRequirement<QuestRequirementState>
    {
        protected QuestRequirementWithDefaultState(string description) : base(description)
        {
        }
    }
}