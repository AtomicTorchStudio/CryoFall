namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Arctic
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropRockArctic : ProtoObjectProp
    {
        public override bool CanFlipSprite => true;

        public override StaticObjectKind Kind => StaticObjectKind.NaturalObject;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.2;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.7, 0.5), offset: (0.15, 0.15));
            AddHalfHeightWallHitboxes(data, width: 0.7, offsetX: 0.15);
        }
    }
}