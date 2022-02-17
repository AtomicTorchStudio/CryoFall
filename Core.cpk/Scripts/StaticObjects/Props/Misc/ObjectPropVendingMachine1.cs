namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropVendingMachine1 : ProtoObjectProp
    {
        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.2;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("#");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1, 0.9), offset: (0, 0.1))
                .AddShapeRectangle((0.9, 0.7),     offset: (0.05, 0.75), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.9, 0.7),     offset: (0.05, 0.85), group: CollisionGroups.HitboxRanged);
        }
    }
}