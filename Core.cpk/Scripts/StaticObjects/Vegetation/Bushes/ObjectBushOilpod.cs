namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Bushes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectBushOilpod : ProtoObjectBush
    {
        public override string Name => "Oilpod";

        protected override TimeSpan TimeToGiveHarvest => TimeSpan.FromHours(3);

        protected override TimeSpan TimeToMature => TimeSpan.FromHours(2);

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureAtlasResource(
                base.PrepareDefaultTexture(thisType),
                columns: 4,
                rows: 1);
        }

        protected override void PrepareGatheringDroplist(DropItemsList droplist)
        {
            droplist
                .Add<ItemOilpod>(count: 1, countRandom: 1)
                .Add<ItemOilpod>(count: 1, probability: 1 / 5.0, condition: SkillForaging.ConditionAdditionalYield);
        }
    }
}