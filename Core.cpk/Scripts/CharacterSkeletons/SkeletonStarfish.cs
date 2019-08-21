namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class SkeletonStarfish : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 0.07;

        public override bool HasStaticAttackAnimations => false;

        public override float OrientationDownExtraAngle => 5;

        // no back-view
        public override SkeletonResource SkeletonResourceBack { get; }
            = null;

        public override SkeletonResource SkeletonResourceFront { get; }
            = new SkeletonResource("Starfish/Front");

        public override double WorldScale => 0.3f;

        protected override string SoundsFolderPath => "Skeletons/Starfish";

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.06 * scaleMultiplier);
            shadowRenderer.Scale = 0.55 * scaleMultiplier;
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
    }
}