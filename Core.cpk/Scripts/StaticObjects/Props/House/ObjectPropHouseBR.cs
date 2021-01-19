namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.House
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropHouseBR : ProtoObjectProp
    {
        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY += 0.75;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("#",
                         "#");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((0.9, 2));

            data.PhysicsBody
                .AddShapeRectangle((0.9, 1.25),
                                   (0, 0 + 0.75),
                                   CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.9, 1.15),
                                   (0, 0 + 0.85),
                                   CollisionGroups.HitboxRanged);
        }
    }
}