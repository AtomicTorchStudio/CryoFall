namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface IProtoItemFertilizer : IProtoItem
    {
        string FertilizerShortDescription { get; }

        double PlantGrowthSpeedMultiplier { get; }
    }
}