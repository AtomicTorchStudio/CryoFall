namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Seeds;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectTreeRubber : ProtoObjectTree
    {
        public override string Name => "Rubber tree";

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
            droplist.Add<ItemLogs>(count: 3);
            droplist.Add<ItemRubberRaw>(count: 3, countRandom: 2);

            // saplings
            droplist.Add<ItemSaplingRubbertree>(count: 1, probability: 0.2);

            // bonus drop
            droplist.Add<ItemLeaf>(count: 2, countRandom: 1, probability: 0.25);
        }
    }
}