namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Seeds;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectTreePineBoreal : ProtoObjectTree
    {
        public override string Name => "Pine tree";

        protected override TimeSpan TimeToMature => TimeSpan.FromHours(3);

        public override byte ClientGetTextureAtlasColumn(
            IStaticWorldObject worldObject,
            VegetationPublicState publicState)
        {
            var growthStage = publicState.GrowthStage;
            if (growthStage == this.GrowthStagesCount)
            {
                // full grown - select one of three variants based on position
                return (byte)(growthStage
                              + PositionalRandom.Get(worldObject.TilePosition, 0, 3, seed: 90139875u));
            }

            return growthStage;
        }

        protected override byte CalculateGrowthStagesCount()
        {
            // last 3 columns are variation of the last growth stage
            return (byte)(base.CalculateGrowthStagesCount() - 2);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.4;
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureAtlasResource(
                base.PrepareDefaultTexture(thisType),
                columns: 5,
                rows: 1);
        }

        protected override void PrepareDroplistOnDestroy(DropItemsList droplist)
        {
            // primary drop
            droplist.Add<ItemLogs>(count: 4);

            // saplings
            droplist.Add<ItemSaplingPineBoreal>(count: 1, probability: 0.15);

            // bonus drop
            droplist.Add<ItemTwigs>(count: 1, countRandom: 2, probability: 0.25);
            droplist.Add<ItemTreebark>(count: 1, countRandom: 1, probability: 0.25);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.25, center: (0.5, 0.47))
                .AddShapeRectangle(size: (0.75, 1),   offset: (0.125, 0.22), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.3, 0.35), offset: (0.35, 1.0),    group: CollisionGroups.HitboxRanged);
        }
    }
}