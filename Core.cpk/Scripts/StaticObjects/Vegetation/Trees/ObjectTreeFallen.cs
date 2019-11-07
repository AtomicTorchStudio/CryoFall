namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectTreeFallen : ProtoObjectTree
    {
        public override string Name => "Fallen tree";

        // fallen tree, so much less hp than normal
        public override float StructurePointsMax => base.StructurePointsMax / 2;

        // no regeneration
        public override double StructurePointsRegenerationDurationSeconds => 0;

        protected override bool CanFlipSprite => false; // because the physics cannot be flipped on the server

        // no growth time
        protected override TimeSpan TimeToMature => TimeSpan.Zero;

        protected override void ClientOnObjectDestroyed(Vector2D position)
        {
            // play default destroy sound
            this.MaterialDestroySoundPreset.PlaySound(
                this.ObjectSoundMaterial,
                this,
                worldPosition: position + this.Layout.Center,
                volume: SoundConstants.VolumeDestroy,
                pitch: RandomHelper.Range(0.95f, 1.05f));
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.Scale = 1f;
            renderer.DrawOrderOffsetY = 0.5;
        }

        protected override void PrepareDroplistOnDestroy(DropItemsList droplist)
        {
            // primary drop
            droplist.Add<ItemLogs>(count: 3);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            // no ranged hitboxes
            data.PhysicsBody
                .AddShapeCircle(radius: 0.2, center: (1.0, 0.3))
                .AddShapeCircle(radius: 0.2, center: (1.0, 0.3), group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.2, center: (0.65, 0.4))
                .AddShapeCircle(radius: 0.2, center: (0.65, 0.4), group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.2, center: (0.3, 0.55))
                .AddShapeCircle(radius: 0.2, center: (0.3, 0.55), group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.2, center: (-0.05, 0.65))
                .AddShapeCircle(radius: 0.2, center: (-0.05, 0.65), group: CollisionGroups.HitboxMelee);
        }
    }
}