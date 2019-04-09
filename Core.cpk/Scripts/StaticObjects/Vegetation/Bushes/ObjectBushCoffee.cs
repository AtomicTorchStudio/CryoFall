namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Bushes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectBushCoffee : ProtoObjectBush
    {
        public override string Name => "Coffee";

        protected override TimeSpan TimeToGiveHarvest => TimeSpan.FromHours(1);

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
                .Add<ItemCoffeeBerries>(count: 3)
                .Add<ItemCoffeeBerries>(count: 2,
                                        probability: 1 / 5.0,
                                        condition: SkillForaging.ConditionAdditionalYield);
        }
    }
}