namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropTentTop : ProtoObjectProp
    {
        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY += 1.5;
            renderer.PositionOffset += (0, -1f);
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (3.0, 0.7), offset: (0.0, 0.0))
                .AddShapeRectangle(size: (2.8, 0.8), offset: (0.1, 0.0), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (2.8, 0.8), offset: (0.1, 0.0), group: CollisionGroups.HitboxRanged);
        }
    }
}