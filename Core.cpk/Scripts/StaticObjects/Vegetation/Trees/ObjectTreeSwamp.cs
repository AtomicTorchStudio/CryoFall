namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Resources;
    using System;

    public class ObjectTreeSwamp : ProtoObjectTree
    {
        public override string Name => "Swamp tree";

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
            droplist.Add<ItemLogs>(count: 4);

            // bonus drop
            droplist.Add<ItemTwigs>(count: 1, countRandom: 2, probability: 0.25);
            droplist.Add<ItemTreebark>(count: 1, countRandom: 1, probability: 0.25);
            droplist.Add<ItemLeaf>(count: 2, countRandom: 1, probability: 0.25);
        }
    }
}