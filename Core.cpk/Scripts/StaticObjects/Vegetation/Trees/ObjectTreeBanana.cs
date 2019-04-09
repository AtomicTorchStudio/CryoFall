namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectTreeBanana : ProtoObjectTree
    {
        public override string Name => "Banana tree";

        protected override TimeSpan TimeToMature => TimeSpan.FromHours(2);

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureAtlasResource(
                base.PrepareDefaultTexture(thisType),
                columns: 4,
                rows: 1);
        }

        protected override void PrepareDroplistOnDestroy(DropItemsList droplist)
        {
            // primary drop
            droplist
                .Add<ItemLogs>(count: 4)
                .Add<ItemTwigs>(count: 1,  countRandom: 2)
                .Add<ItemBanana>(count: 1, countRandom: 1);

            // bonus drop
            droplist
                .Add<ItemLeaf>(count: 2, countRandom: 1, probability: 0.5);

            // skill
            droplist
                .Add<ItemBanana>(count: 1, probability: 1 / 5.0, condition: SkillForaging.ConditionAdditionalYield);
        }
    }
}