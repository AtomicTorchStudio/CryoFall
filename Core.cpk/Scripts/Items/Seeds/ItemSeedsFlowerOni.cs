namespace AtomicTorch.CBND.CoreMod.Items.Seeds
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;

    public class ItemSeedsFlowerOni : ProtoItemSeed
    {
        public override string Description => "Oniflower seeds. Can be planted to grow more flowers.";

        public override string Name => "Oniflower seeds";

        protected override void PrepareProtoItemSeed(
            out IProtoObjectVegetation objectPlantProto,
            List<IProtoObjectFarm> allowedToPlaceAt)
        {
            objectPlantProto = GetPlant<ObjectPlantFlowerOni>();

            allowedToPlaceAt.Add(GetPlot<ObjectFarmPlot>());
            allowedToPlaceAt.Add(GetPlot<ObjectPlantPot>());
        }
    }
}