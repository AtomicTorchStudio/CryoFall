﻿namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Walls
{
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropRuinsWallX : ProtoObjectProp
    {
        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.5;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.7, 1), offset: (0.15, 0))
                .AddShapeRectangle(size: (0.15, 0.65))
                .AddShapeRectangle(size: (0.15, 0.65), offset: (0.85, 0));
            AddFullHeightWallHitboxes(data);
        }
    }
}