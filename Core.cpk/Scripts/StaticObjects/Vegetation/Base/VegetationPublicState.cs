namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class VegetationPublicState : StaticObjectPublicState
    {
        [SyncToClient]
        public byte GrowthStage { get; set; }

        public virtual bool IsFullGrown(IProtoObjectVegetation protoObjectVegetation)
        {
            return this.GrowthStage == protoObjectVegetation.GrowthStagesCount;
        }
    }
}