﻿namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectTreeStumpMossy : ProtoObjectTree
    {
        public override string Name => "Mossy stump";

        // stump, so much less hp than normal
        public override float StructurePointsMax => base.StructurePointsMax / 4;

        // no regeneration
        public override double StructurePointsRegenerationDurationSeconds => 0;

        public override double TreeHeight => 0.5;

        // no growth time
        protected override TimeSpan TimeToMature => TimeSpan.Zero;

        protected override void ClientOnObjectDestroyed(Vector2D position)
        {
            // play default destroy sound
            this.MaterialDestroySoundPreset.PlaySound(
                this.ObjectMaterial,
                this,
                worldPosition: position + this.Layout.Center,
                volume: SoundConstants.VolumeDestroy,
                pitch: RandomHelper.Range(0.95f, 1.05f));
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.Scale = 1.1f;
            renderer.PositionOffset += (0, 0.1);
            renderer.DrawOrderOffsetY = 0.3;
        }

        protected override void PrepareDroplistOnDestroy(DropItemsList droplist)
        {
            // primary drop
            droplist.Add<ItemLogs>(count: 3);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.2, center: (0.5, 0.35))
                .AddShapeCircle(radius: 0.2, center: (0.5, 0.35), group: CollisionGroups.HitboxMelee);
        }
    }
}