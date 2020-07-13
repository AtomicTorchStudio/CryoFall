namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation
{
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectVegetation : IProtoStaticWorldObject
    {
        IReadOnlyDropItemsList DroplistOnDestroy { get; }

        byte GrowthStagesCount { get; }

        float CalculateShadowScale(VegetationClientState clientState);

        byte ClientGetTextureAtlasColumn(IStaticWorldObject worldObject, VegetationPublicState publicState);

        double GetGrowthStageDurationSeconds(byte growthStage);

        void ServerSetFullGrown(IStaticWorldObject worldObject);

        /// <summary>
        /// Sets growth progress.
        /// </summary>
        /// <param name="worldObject">Object with ProtoVegetation prototype.</param>
        /// <param name="progress">Value from 0 (new vegetation) to 1 (full grown)</param>
        void ServerSetGrowthProgress(IStaticWorldObject worldObject, double progress);

        void ServerSetGrowthStage(IStaticWorldObject worldObject, byte growthStage);

        double SharedGetGrowthProgress(IWorldObject worldObject);
    }
}