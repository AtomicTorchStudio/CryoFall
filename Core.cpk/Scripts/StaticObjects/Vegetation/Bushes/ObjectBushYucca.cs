namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Bushes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectBushYucca : ProtoObjectBush
    {
        public override string Name => "Yucca";

        protected override string InteractionFailedNoFruitsMessage => ObjectBushWaterbulb.ErrorNoFruit;

        protected override TimeSpan TimeToGiveHarvest => TimeSpan.FromHours(3);

        protected override TimeSpan TimeToMature => TimeSpan.FromHours(2);

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureAtlasResource(
                base.PrepareDefaultTexture(thisType),
                columns: 5,
                rows: 1);
        }

        protected override void PrepareGatheringDroplist(DropItemsList droplist)
        {
            droplist
                .Add<ItemYucca>(count: 1)
                .Add<ItemYucca>(count: 1, probability: 1 / 5.0, condition: SkillForaging.ConditionAdditionalYield);
        }
    }
}