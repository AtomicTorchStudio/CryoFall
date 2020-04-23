namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonChicken : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 1.5;

        public override Vector2D IconOffset => (0, -15);

        public override double IconScale => 0.8;

        public override float OrientationDownExtraAngle => 5;

        public override SkeletonResource SkeletonResourceBack { get; }
            = new SkeletonResource("Chicken/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new SkeletonResource("Chicken/Front");

        public override double WorldScale => 0.4;

        protected override string SoundsFolderPath => "Skeletons/Chicken";

        protected override double VolumeFootsteps => 0.33;

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.01 * scaleMultiplier);
            shadowRenderer.Scale = 0.35 * scaleMultiplier;
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody
                .AddShapeRectangle(size: (0.4, 0.2),
                                   offset: (-0.2, -0.05))
                .AddShapeCircle(radius: 0.3,
                                center: (0, 0.25),
                                group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.3,
                                center: (0, 0.25),
                                group: CollisionGroups.HitboxRanged);
        }
    }
}