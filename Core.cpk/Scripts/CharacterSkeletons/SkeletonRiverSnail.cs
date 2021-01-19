namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class SkeletonRiverSnail : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 0.1;

        public override double IconScale => 0.9;

        public override float OrientationDownExtraAngle => 5;

        public override SkeletonResource SkeletonResourceBack { get; }
            = new("RiverSnail/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("RiverSnail/Front");

        public override double WorldScale => 0.4;

        protected override string SoundsFolderPath => "Skeletons/RiverSnail";

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.01 * scaleMultiplier);
            shadowRenderer.Scale = 0.35 * scaleMultiplier;
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody
                .AddShapeCircle(radius: 0.2,
                                center: (0, 0.125))
                .AddShapeCircle(
                    radius: 0.3,
                    center: (0, 0.25),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(
                    radius: 0.3,
                    center: (0, 0.25),
                    group: CollisionGroups.HitboxRanged);
        }
    }
}