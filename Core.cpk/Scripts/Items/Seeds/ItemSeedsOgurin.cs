namespace AtomicTorch.CBND.CoreMod.Items.Seeds
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;

    public class ItemSeedsOgurin : ProtoItemSeed
    {
        public override string Description =>
            "Ogurin was one of the very first plants specifically engineered for this planet.";

        public override string Name => "Ogurin seeds";

        protected override void PrepareProtoItemSeed(
            out IProtoObjectVegetation objectPlantProto,
            List<IProtoObjectFarm> allowedToPlaceAt)
        {
            objectPlantProto = GetPlant<ObjectPlantOgurin>();

            allowedToPlaceAt.Add(GetPlot<ObjectFarmPlot>());
        }
    }
}