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

        double ClientCalculateHarvestTotalDuration(bool onlyForHarvestStage);

        Task<ProtoPlantTooltipPrivateData> ClientGetTooltipData(IStaticWorldObject plant);

        void ServerOnWatered(ICharacter byCharacter, IStaticWorldObject worldObjectPlant, double wateringDuration);

        void ServerRefreshCurrentGrowthDuration(IStaticWorldObject worldObjectPlant);

        void ServerSetBonusForCharacter(IStaticWorldObject plantObject, ICharacter character);
    }
}