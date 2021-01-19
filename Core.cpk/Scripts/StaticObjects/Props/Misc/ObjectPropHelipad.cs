namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropHelipad : ProtoObjectProp
    {
        public GroundSoundMaterial GroundSoundMaterial => GroundSoundMaterial.Stone;

        public override StaticObjectKind Kind => StaticObjectKind.Floor;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);

            renderer.DrawOrder = DrawOrder.Floor;
            renderer.PositionOffset = (0.0, 0.3);
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("######",
                         "######",
                         "######",
                         "######");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
        }
    }
}