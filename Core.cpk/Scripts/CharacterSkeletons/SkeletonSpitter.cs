namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonSpitter : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 0;

        public override Vector2D IconOffset => (60, -120);

        public override double IconScale => 0.5;

        public override SkeletonResource SkeletonResourceBack
            => null;

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("Spitter/Front");

        public override double WorldScale => 0.5;

        protected override string SoundsFolderPath => "Skeletons/Spitter";

        public override float CalculateAimAnimationPosition(double angleDeg, ViewOrientation viewOrientation)
        {
            angleDeg += 90;
            angleDeg %= 360;
            if (angleDeg > 180)
            {
                // flip horizontally
                angleDeg = 360 - angleDeg;
            }

            return (float)(angleDeg / 180.0);
        }

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.025 * scaleMultiplier);
            shadowRenderer.Scale = (1 * scaleMultiplier, 1.5 * scaleMultiplier);
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody.IsNotPushable = true;

            physicsBody
                .AddShapeCircle(radius: 0.185, center: (0, 0))
                .AddShapeCircle(radius: 0.15,  center: (-0.16, -0.025))
                .AddShapeCircle(radius: 0.15,  center: (0.16, -0.025))
                .AddShapeRectangle(size: (0.3, 1.1), offset: (-0.3 / 2.0, 0),    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.3, 1.2), offset: (-0.3 / 2.0, 0.05), group: CollisionGroups.HitboxRanged);
        }

        public override void OnSkeletonCreated(IComponentSkeleton skeleton)
        {
            base.OnSkeletonCreated(skeleton);
            skeleton.SetMixDuration(null, "Shot", secondsMixAB: 0, 0);
            skeleton.PositionOffset += (0, -0.12);
        }
    }
}