namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Walls
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ObjectPropRuinsWallGateBroken : ProtoObjectProp
    {
        public override bool CanFlipSprite => false;

        public override ObjectMaterial ObjectMaterial
            => ObjectMaterial.Metal;

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.5, 0.75))
                .AddShapeRectangle(size: (0.5, 0.75), offset: (1.5, 0.0));

            AddHalfHeightWallHitboxes(data, width: 0.5, offsetY: -0.15);
            AddHalfHeightWallHitboxes(data, width: 0.5, offsetX: 1.5, offsetY: -0.15);
        }
    }
}