namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Road
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropRoadUp : ProtoObjectPropPlatform
    {
        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset += (-1, 0);
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##",
                         "##",
                         "##");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            var width = 0.56;
            data.PhysicsBody
                .AddShapeRectangle((width, 2.55), (3.0 - 2 - width - 1, 1.1), CollisionGroups.Default)
                .AddShapeRectangle((width, 2.55), (3.0 - 1, 1.1),             CollisionGroups.Default);
        }
    }
}