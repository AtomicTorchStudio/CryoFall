namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.SmallGatherables
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectSmallMushroomRust : ProtoObjectSmallGatherableVegetation
    {
        public override string Name => "Rustshroom";

        protected override TimeSpan TimeToMature => TimeSpan.Zero;

        public override byte ClientGetTextureAtlasColumn(
            IStaticWorldObject worldObject,
            VegetationPublicState publicState)
        {
            var growthStage = publicState.GrowthStage;
            if (growthStage == this.GrowthStagesCount)
            {
                // full grown - select one of two variants based on position
                return (byte)(growthStage
                              + PositionalRandom.Get(worldObject.TilePosition, 0, 2, seed: 234598675u));
            }

            return growthStage;
        }

        protected override byte CalculateGrowthStagesCount()
        {
            // last column is not a growth stage - is just another variant of texture (several variations of fully grown plant).
            return (byte)(base.CalculateGrowthStagesCount() - 1);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset += (0, 0.1);
            renderer.DrawOrderOffsetY = 0.26;
        }

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
                .Add<ItemMushroomRust>(count: 1)
                .Add<ItemMushroomRust>(count: 1,
                                       probability: 1 / 5.0,
                                       condition: SkillForaging.ConditionAdditionalYield);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(offset: (0.25, 0.2), size: (0.5, 0.6), group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.45, center: (0.5, 0.5), group: CollisionGroups.ClickArea);
            // no ranged hitbox
        }
    }
}