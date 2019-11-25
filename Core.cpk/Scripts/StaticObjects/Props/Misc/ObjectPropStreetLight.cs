namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropStreetLight : ProtoObjectProp
    {
        public override bool CanFlipSprite => true;

        public override ObjectMaterial ObjectMaterial
            => ObjectMaterial.Metal;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.4;
            renderer.PositionOffset = (0.5, 0);
            renderer.SpritePivotPoint = (0.5, 0);
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("#");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.8, 0.5),  offset: (0.1, 0.2))
                .AddShapeRectangle(size: (0.45, 0.4), offset: (0.275, 0.75), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.4, 0.2),  offset: (0.3, 0.95),   group: CollisionGroups.HitboxRanged);
        }
    }
}