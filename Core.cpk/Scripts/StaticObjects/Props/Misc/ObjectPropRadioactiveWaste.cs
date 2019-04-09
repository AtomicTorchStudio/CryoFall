namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropRadioactiveWaste : ProtoObjectProp
    {
        public override bool CanFlipSprite => true;

        public override ObjectSoundMaterial ObjectSoundMaterial
            => ObjectSoundMaterial.Metal;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.65;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup(
                "##");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1.7, 1.0), offset: (0.15, 0.0));
        }
    }
}