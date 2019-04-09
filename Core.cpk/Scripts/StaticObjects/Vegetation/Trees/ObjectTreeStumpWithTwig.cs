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

    public class ObjectTreeStumpWithTwig : ProtoObjectTree
    {
        public override string Name => "Tree stump";

        // stump, so much less hp than normal
        public override float StructurePointsMax => base.StructurePointsMax / 4;

        // no regeneration
        public override double StructurePointsRegenerationDurationSeconds => 0;

        // no growth time
        protected override TimeSpan TimeToMature => TimeSpan.Zero;

        protected override void ClientOnObjectDestroyed(Vector2Ushort tilePosition)
        {
            // play default destroy sound
            this.MaterialDestroySoundPreset.PlaySound(
                this.ObjectSoundMaterial,
                this,
                worldPosition: tilePosition.ToVector2D() + this.Layout.Center,
                volume: SoundConstants.VolumeDestroy,
                pitch: RandomHelper.Range(0.95f, 1.05f));
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.Scale = 1.1;
            renderer.DrawOrderOffsetY = 0.3;
        }

        protected override void PrepareDroplistOnDestroy(DropItemsList droplist)
        {
            // primary drop
            droplist.Add<ItemLogs>(count: 1);
            droplist.Add<ItemTwigs>(count: 1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.2, center: (0.5, 0.3), group: CollisionGroups.Default)
                .AddShapeCircle(radius: 0.2, center: (0.5, 0.3), group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.2, center: (0.5, 0.3), group: CollisionGroups.HitboxRanged);
        }
    }
}