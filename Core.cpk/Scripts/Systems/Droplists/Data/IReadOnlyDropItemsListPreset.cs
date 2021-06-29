namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    public interface IReadOnlyDropItemsListPreset
    {
        IReadOnlyDropItemsList DropItemsList { get; }

        DropItemConditionDelegate CreateCompoundConditionIfNecessary(DropItemConditionDelegate otherCondition);

        double GetProbabilityForDroplist();
        
        double GetCountMultiplierForDroplist();
    }
}