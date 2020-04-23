namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonPragmiumBeetle : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 1.35;

        public override Vector2D IconOffset => (-10, 15);

        public override double IconScale => 0.8;

        public override SkeletonResource SkeletonResourceBack { get; }
            = new SkeletonResource("PragmiumBeetle/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new SkeletonResource("PragmiumBeetle/Front");

        public override double WorldScale => 0.4;

        protected override string SoundsFolderPath => "Skeletons/Beetle";

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.1 * scaleMultiplier);
            shadowRenderer.Scale = 0.6 * scaleMultiplier;
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody
                .AddShapeRectangle(size: (0.55, 0.25),
                                   offset: (-0.25, -0.05))
                .AddShapeCircle(radius: 0.4,
                                center: (0, 0.2),
                                group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.4,
                                center: (0, 0.2),
                                group: CollisionGroups.HitboxRanged);
        }
    }
}