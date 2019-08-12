namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropFlowerbedVertical : ProtoObjectProp
    {
        public override bool CanFlipSprite => true;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 2.5;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("#",
                         "#",
                         "#");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 2.5), offset: (0, 0.1))
                .AddShapeRectangle((0.8, 2),       offset: (0.1, 0.75),    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.8, 2.27),    offset: (0.1, 0.85), group: CollisionGroups.HitboxRanged);
        }
    }
}