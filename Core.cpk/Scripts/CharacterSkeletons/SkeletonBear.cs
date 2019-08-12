namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class SkeletonBear : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 0.8;

        public override SkeletonResource SkeletonResourceBack { get; }
            = new SkeletonResource("Bear/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new SkeletonResource("Bear/Front");

        public override double WorldScale => 0.45;

        protected override string SoundsFolderPath => "Skeletons/Bear";

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.2 * scaleMultiplier);
            shadowRenderer.Scale = 1.5 * scaleMultiplier;
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody
                .AddShapeRectangle(size: (0.8, 0.4),
                                   offset: (-0.4, -0.25))
                .AddShapeCircle(radius: 0.55,
                                center: (0, 0.35),
                                group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.55,
                                center: (0, 0.35),
                                group: CollisionGroups.HitboxRanged);
        }
    }
}