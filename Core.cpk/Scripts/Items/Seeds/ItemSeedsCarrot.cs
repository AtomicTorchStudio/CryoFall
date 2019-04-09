namespace AtomicTorch.CBND.CoreMod.Items.Seeds
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;

    public class ItemSeedsCarrot : ProtoItemSeed
    {
        public override string Description => "Can be planted to grow carrots.";

        public override string Name => "Carrot seeds";

        protected override void PrepareProtoItemSeed(
            out IProtoObjectVegetation objectPlantProto,
            List<IProtoObjectFarm> allowedToPlaceAt)
        {
            objectPlantProto = GetPlant<ObjectPlantCarrot>();

            allowedToPlaceAt.Add(GetPlot<ObjectFarmPlot>());
        }
    }
}