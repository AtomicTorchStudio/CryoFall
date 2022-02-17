namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonBroodGuardian : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 0.778;

        public override Vector2D IconOffset => (0, -30);

        public override double IconScale => 0.8;

        public override SkeletonResource SkeletonResourceBack { get; }
            = new("BroodGuardian/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("BroodGuardian/Front");

        public override double WorldScale => 0.7;

        protected override string SoundsFolderPath => "Skeletons/Spitter"; // reusing the sounds from Spitter

        protected override double VolumeFootsteps => 0.5;

        public override void ClientSetupShadowRenderer(
            IComponentSpriteRenderer shadowRenderer,
            double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, 0.01 * scaleMultiplier);
            shadowRenderer.Scale = 1.25 * scaleMultiplier;
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody
                .AddShapeCircle(radius: 0.25,
                                center: (0, 0))
                .AddShapeRectangle(size: (0.7, 0.7),
                                   offset: (-0.35, 0.25),
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.6, 1.1),
                                   offset: (-0.3, 0),
                                   group: CollisionGroups.HitboxRanged);
        }
    }
}