namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropBulldozer : ProtoObjectProp
    {
        public override ObjectMaterial ObjectMaterial
            => ObjectMaterial.Metal;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY += 0.7;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup(
                "####",
                "####");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (2.9, 1.2), offset: (0.1, 0.2));
            AddFullHeightWallHitboxes(data, width: 2.9, offsetX: 0.2);
        }
    }
}