namespace AtomicTorch.CBND.CoreMod.Items.Seeds
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;

    public class ItemSaplingOak : ProtoItemSapling, IProtoItemOrganic
    {
        public override string Description => GetProtoEntity<ItemSaplingBirch>().Description;

        public override string Name => "Oak tree sapling";

        public ushort OrganicValue => 2;

        protected override void PrepareProtoItemSapling(out IProtoObjectVegetation objectPlantProto)
        {
            objectPlantProto = GetPlant<ObjectTreeOak>();
        }
    }
}