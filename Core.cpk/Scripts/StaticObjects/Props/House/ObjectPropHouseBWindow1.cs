namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.House
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ObjectPropHouseBWindow1 : ProtoObjectProp
    {
        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("#",
                         "#");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((1.0, 2),    (0.0, 0))
                .AddShapeRectangle((1.0, 1.25), (0.0, 0.75), CollisionGroups.HitboxMelee)
                .AddShapeRectangle((1.0, 1.15), (0.0, 0.85), CollisionGroups.HitboxRanged);
        }
    }
}