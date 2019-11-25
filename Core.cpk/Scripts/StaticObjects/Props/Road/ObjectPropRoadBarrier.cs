namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Road
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropRoadBarrier : ProtoObjectProp
    {
        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            Client.Rendering.CreateSpriteRenderer(
                data.GameObject,
                new TextureResource("StaticObjects/Props/Road/ObjectPropRoadVertical"),
                DrawOrder.Floor,
                positionOffset: (1.0, 0.0));
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset = (0.85, 0.6);
            //renderer.DrawOrder = DrawOrder.Floor;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("####",
                         "####");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (2.7, 0.4),  offset: (0.85, 0.6))
                .AddShapeRectangle(size: (2.7, 0.6),  offset: (0.85, 0.7), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.65, 0.4), offset: (2.9, 1.3),  group: CollisionGroups.HitboxRanged);
        }
    }
}