namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectLootPileMinerals : ProtoObjectLootContainer
    {
        public override double DurationGatheringSeconds => 2;

        public override string InteractionTooltipText => InteractionTooltipTexts.Gather;

        public override bool IsAutoTakeAll => true;

        public override string Name => "Minerals pile";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override double ObstacleBlockDamageCoef => 0.5;

        // It's much easier to find and search these piles so they will provide much less skill experience.
        public override double SearchingSkillExperienceMultiplier => 0.2;

        public override float StructurePointsMax => 0;

        protected override bool CanFlipSprite => true;

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
            => (0.5, 0.3);

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.5;
        }

        protected override void PrepareLootDroplist(DropItemsList droplist)
        {
            // common loot
            droplist.Add(nestedList:
                         new DropItemsList(outputs: 1)
                             // resources
                             .Add<ItemOreCopper>(count: 20, countRandom: 5)
                             .Add<ItemOreIron>(count: 20, countRandom: 5)
                             .Add<ItemSulfurPowder>(count: 20, countRandom: 5)
                             .Add<ItemPotassiumNitrate>(count: 20, countRandom: 5));
        }

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetObject()
        {
            return ObjectsSoundsPresets.ObjectLootPile;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.32, center: (0.5, 0.35))
                .AddShapeCircle(radius: 0.35, center: (0.5, 0.35), group: CollisionGroups.ClickArea);
        }
    }
}