namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Seeds;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectTreeOak : ProtoObjectTree
    {
        public override string Name => "Oak tree";

        public override double TreeHeight => 1.8;

        protected override TimeSpan TimeToMature => TimeSpan.FromHours(2);

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureAtlasResource(
                base.PrepareDefaultTexture(thisType),
                columns: 3,
                rows: 1);
        }

        protected override void PrepareDroplistOnDestroy(DropItemsList droplist)
        {
            // primary drop
            droplist
                .Add<ItemLogs>(count: 5)
                .Add<ItemTwigs>(count: 2, countRandom: 2);

            // saplings drop (requires skill)
            droplist
                .Add<ItemSaplingOak>(condition: SkillLumbering.ConditionGetSapplings,      count: 1, probability: 0.1)
                .Add<ItemSaplingOak>(condition: SkillLumbering.ConditionGetExtraSapplings, count: 1, probability: 0.1);
        }
    }
}