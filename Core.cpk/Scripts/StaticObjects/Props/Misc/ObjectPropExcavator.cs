namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropExcavator : ProtoObjectProp
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
                .AddShapeRectangle(size: (2.3, 1.2), offset: (0.2, 0.2))
                .AddShapeRectangle(size: (1.0, 0.7), offset: (2.5, 0.3));
            
            AddHalfHeightWallHitboxes(data, width: 2.3, offsetX: 0.2, offsetY: 0.2);
            AddHalfHeightWallHitboxes(data, width: 1.0, offsetX: 2.5, offsetY: 0.3);
        }
    }
}