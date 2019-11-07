namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props.Misc
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectPropBillboard1 : ProtoObjectProp
    {
        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            Client.Rendering.CreateSpriteRenderer(
                data.GameObject,
                new TextureResource("StaticObjects/Props/Misc/ObjectPropBillboardShadow"),
                DrawOrder.Floor);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY += 0.4;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1.6, 0.3), offset: (0.2, 0.1))
                .AddShapeRectangle(size: (0.4, 0.7), offset: (0.8, 0.1), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.4, 0.4), offset: (0.8, 0.8), group: CollisionGroups.HitboxRanged);
        }
    }
}