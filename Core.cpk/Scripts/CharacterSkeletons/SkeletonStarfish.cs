namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonStarfish : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 0.1;

        public override bool HasStaticAttackAnimations => false;

        public override Vector2D IconOffset => (0, 25);

        public override double IconScale => 0.6;

        public override float OrientationDownExtraAngle => 5;

        // no back-view
        public override SkeletonResource SkeletonResourceBack { get; }
            = null;

        public override SkeletonResource SkeletonResourceFront { get; }
            = new SkeletonResource("Starfish/Front");

        public override double WorldScale => 0.3;

        protected override string SoundsFolderPath => "Skeletons/Starfish";

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.IsEnabled = false;
            shadowRenderer.Scale = 0;
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody
                .AddShapeCircle(radius: 0.25,
                                center: (0, 0.0))
                .AddShapeCircle(radius: 0.4,
                                center: (0, 0.25),
                                group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.4,
                                center: (0, 0.25),
                                group: CollisionGroups.HitboxRanged);
        }

        public override void OnSkeletonCreated(IComponentSkeleton skeleton)
        {
            base.OnSkeletonCreated(skeleton);

            skeleton.DrawOrder = DrawOrder.Shadow - 1;
        }
    }
}