namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonBear : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 1.2;

        public override Vector2D IconOffset => (0, -5);

        public override double IconScale => 0.5;

        public override SkeletonResource SkeletonResourceBack { get; }
            = new("Bear/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("Bear/Front");

        public override double WorldScale => 0.45;

        protected override string SoundsFolderPath => "Skeletons/Bear";

        protected override double VolumeFootsteps => 1.0;

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.2 * scaleMultiplier);
            shadowRenderer.Scale = 1.5 * scaleMultiplier;
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody
                .AddShapeCircle(radius: 0.4,
                                center: (0, 0))
                .AddShapeCircle(radius: 0.55,
                                center: (0, 0.35),
                                group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.55,
                                center: (0, 0.35),
                                group: CollisionGroups.HitboxRanged);
        }
    }
}