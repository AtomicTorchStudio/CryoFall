namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectPropBoatWreck : ProtoObjectProp
    {
        public override bool HasIncreasedScopeSize => true;

        public override ObjectMaterial ObjectMaterial
            => ObjectMaterial.Metal;

        public override BoundsInt ViewBoundsExpansion => new(minX: -3,
                                                             minY: -3,
                                                             maxX: 3,
                                                             maxY: 3);

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = +1.5;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup(
                "#####",
                "#####",
                "#####");
        }

        protected override void PrepareTileRequirements(ConstructionTileRequirements tileRequirements)
        {
            tileRequirements.Clear();
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (2.4, 0.7), offset: (2.0, 1.2), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (2.4, 0.7), offset: (2.0, 1.0), group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.8, center: (2.4, 1.2))
                .AddShapeCircle(radius: 0.7, center: (3.1, 1.4))
                .AddShapeCircle(radius: 0.6, center: (3.6, 1.5))
                .AddShapeCircle(radius: 0.6, center: (4.1, 1.6));
        }
    }
}