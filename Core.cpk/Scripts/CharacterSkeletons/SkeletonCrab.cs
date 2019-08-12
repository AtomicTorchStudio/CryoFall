namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class SkeletonCrab : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 0.5;

        public override bool HasStaticAttackAnimations => false;

        // no back-view
        public override SkeletonResource SkeletonResourceBack { get; }
            = null;

        public override SkeletonResource SkeletonResourceFront { get; }
            = new SkeletonResource("Crab/Front");

        public override double WorldScale => 0.4f;

        protected override string SoundsFolderPath => "Skeletons/Crab";

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.06 * scaleMultiplier);
            shadowRenderer.Scale = 0.7 * scaleMultiplier;
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody
                .AddShapeRectangle(size: (0.6, 0.25),
                                   offset: (-0.3, -0.15))
                .AddShapeCircle(radius: 0.4,
                                center: (0, 0.25),
                                group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.4,
                                center: (0, 0.25),
                                group: CollisionGroups.HitboxRanged);
        }
    }
}