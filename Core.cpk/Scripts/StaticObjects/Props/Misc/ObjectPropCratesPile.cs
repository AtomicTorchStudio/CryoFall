namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropCratesPile : ProtoObjectProp
    {
        public override bool CanFlipSprite => true;

        public override ObjectSoundMaterial ObjectSoundMaterial
            => ObjectSoundMaterial.Wood;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.4;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1.8, 1.2), offset: (0.1, 0.1));
        }
    }
}