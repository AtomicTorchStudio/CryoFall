namespace AtomicTorch.CBND.CoreMod.Items.Seeds
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;

    public class ItemSeedsBellPepper : ProtoItemSeed
    {
        public override string Description => "Small bag of seeds prepared for planting. Can be planted in the soil.";

        public override string Name => "Bell pepper seeds";

        protected override void PrepareProtoItemSeed(
            out IProtoObjectVegetation objectPlantProto,
            List<IProtoObjectFarm> allowedToPlaceAt)
        {
            objectPlantProto = GetPlant<ObjectPlantBellPepper>();

            allowedToPlaceAt.Add(GetPlot<ObjectFarmPlot>());
        }
    }
}