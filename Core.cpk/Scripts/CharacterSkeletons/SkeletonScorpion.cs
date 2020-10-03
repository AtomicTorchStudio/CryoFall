namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonScorpion : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 2.25;

        public override Vector2D IconOffset => (-55, 30);

        public override double IconScale => 0.46;

        public override SkeletonResource SkeletonResourceBack { get; }
            = new SkeletonResource("Scorpion/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new SkeletonResource("Scorpion/Front");

        public override double WorldScale => 0.4;

        protected override string SoundsFolderPath => "Skeletons/Scorpion";

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.15 * scaleMultiplier);
            shadowRenderer.Scale = 1.75 * scaleMultiplier;
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody
                .AddShapeCircle(radius: 0.35,
                                center: (0 - 0.125, 0.1))
                .AddShapeCircle(radius: 0.35,
                                center: (0 + 0.125, 0.1))
                .AddShapeCircle(radius: 0.5,
                                center: (0, 0.15),
                                group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.45, // carefully selected to ensure no damage through a wall below
                                center: (0, 0.15),
                                group: CollisionGroups.HitboxRanged);
        }

        public override void OnSkeletonCreated(IComponentSkeleton skeleton)
        {
            base.OnSkeletonCreated(skeleton);
            skeleton.DrawOrderOffsetY = -0.2;
        }
    }
}