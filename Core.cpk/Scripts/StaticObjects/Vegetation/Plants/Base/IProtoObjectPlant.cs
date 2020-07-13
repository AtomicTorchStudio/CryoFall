namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants
{
    using System.Threading.Tasks;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IProtoObjectPlant : IProtoObjectGatherableVegetation
    {
        ITextureResource IconFullGrown { get; }

        byte NumberOfHarvests { get; }

        double TimeToGiveHarvestTotalSeconds { get; }

        double TimeToHarvestSpoilTotalSeconds { get; }

        double ClientCalculateHarvestTotalDuration(bool onlyForHarvestStage);

        Task<ProtoPlantTooltipPrivateData> ClientGetTooltipData(IStaticWorldObject plant);

        bool ServerCanBeWatered(IStaticWorldObject worldObjectPlant);

        void ServerOnWatered(
            IStaticWorldObject worldObjectPlant,
            double wateringDuration,
            ICharacter byCharacter = null);

        void ServerRefreshCurrentGrowthDuration(IStaticWorldObject worldObjectPlant);

        void ServerSetBonusForCharacter(IStaticWorldObject plantObject, ICharacter character);
    }
}