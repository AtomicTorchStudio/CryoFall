namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropFountain : ProtoObjectProp
    {
        public override ObjectMaterial ObjectMaterial
            => ObjectMaterial.Stone;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY += 1.0;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup(
                "###",
                "###");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1.4, 1.6), offset: (0.8, 0.15))
                .AddShapeCircle(radius: 0.8, center: (0.8, 0.95))
                .AddShapeCircle(radius: 0.8, center: (2.2, 0.95))
                .AddShapeRectangle(size: (0.8, 1.0), offset: (1.1, 0.8), group: CollisionGroups.HitboxRanged);
        }
    }
}