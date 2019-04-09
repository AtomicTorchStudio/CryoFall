namespace AtomicTorch.CBND.CoreMod.Quests
{
    public interface IQuestRequirementWithCount : IQuestRequirement
    {
        ushort RequiredCount { get; }
    }
}