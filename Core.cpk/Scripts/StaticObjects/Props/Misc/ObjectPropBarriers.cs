namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropBarriers : ProtoObjectProp
    {
        public override bool CanFlipSprite => true;

        public override ObjectMaterial ObjectMaterial
            => ObjectMaterial.Metal;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY += 0.2;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1.8, 0.3), offset: (0.1, 0.1))
                .AddShapeRectangle(size: (1.8, 0.4), offset: (0.1, 0.1), group: CollisionGroups.HitboxMelee);

            // no ranged hitbox here
        }
    }
}