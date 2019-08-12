namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class SkeletonBlackBeetle : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 0.9;

        public override SkeletonResource SkeletonResourceBack { get; }
            = new SkeletonResource("BlackBeetle/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new SkeletonResource("BlackBeetle/Front");

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
                .AddShapeRectangle(size: (0.6, 0.25),
                                   offset: (-0.3, -0.05))
                .AddShapeCircle(radius: 0.4,
                                center: (0, 0.35),
                                group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.4,
                                center: (0, 0.35),
                                group: CollisionGroups.HitboxRanged);
        }
    }
}