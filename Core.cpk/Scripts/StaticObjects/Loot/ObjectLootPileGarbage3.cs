namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectLootPileGarbage3 : ProtoObjectLootContainer
    {
        public override double DurationGatheringSeconds => 1;

        public override string InteractionTooltipText => InteractionTooltipTexts.Examine;

        public override bool IsAutoTakeAll => true;

        public override string Name => GetProtoEntity<ObjectLootPileGarbage1>().Name;

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.HardTissues;

        public override double ObstacleBlockDamageCoef => 0.5;

        // It's much easier to find and search these piles so they will provide much less skill experience.
        public override double SearchingSkillExperienceMultiplier => 0.2;

        public override float StructurePointsMax => 1000;

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
            droplist.Add(nestedList: new DropItemsList(outputs: 1)
                                     .Add<ItemPlastic>(count: 1));

            // extra loot
            droplist.Add(condition: SkillSearching.ServerRollExtraLoot,
                         nestedList: new DropItemsList(outputs: 1)
                                     .Add<ItemPlastic>(count: 1)
                                     .Add<ItemBottleEmpty>(count: 1)
                                     .Add<ItemCoinPenny>(count: 1, countRandom: 2));
        }

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetObject()
        {
            return ObjectsSoundsPresets.ObjectGarbagePile;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.32, center: (0.5, 0.35))
                .AddShapeCircle(radius: 0.32, center: (0.5, 0.4), group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.3, center: (0.5, 0.45), group: CollisionGroups.HitboxRanged)
                .AddShapeCircle(radius: 0.35, center: (0.5, 0.35), group: CollisionGroups.ClickArea);
        }

    }
}