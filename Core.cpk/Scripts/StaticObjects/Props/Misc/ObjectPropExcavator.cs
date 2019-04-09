namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropExcavator : ProtoObjectProp
    {
        public override ObjectSoundMaterial ObjectSoundMaterial
            => ObjectSoundMaterial.Metal;

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
                .AddShapeRectangle(size: (2.4, 1.2), offset: (0.1, 0.1))
                .AddShapeRectangle(size: (1.0, 0.7), offset: (2.5, 0.3));
        }
    }
}