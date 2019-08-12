namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Road
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropSignStopFront : ProtoObjectProp
    {
        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY += 0.25;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.4, 0.4),  offset: (0.3, 0))
                .AddShapeRectangle(size: (0.35, 0.4), offset: (0.325, 0.75), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.3, 0.2),  offset: (0.35, 0.95),  group: CollisionGroups.HitboxRanged);
        }
    }
}