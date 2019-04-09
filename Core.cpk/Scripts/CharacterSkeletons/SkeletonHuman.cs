namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public abstract class SkeletonHuman : ProtoCharacterSkeleton
    {
        public override double DefaultMoveSpeed => 1.5;

        public override bool HasMoveStartAnimations => true;

        public override float OrientationDownExtraAngle => 35;

        public override float OrientationThresholdDownHorizontalFlipDeg => 25;

        public override float OrientationThresholdDownToUpFlipDeg => 45;

        public override float OrientationThresholdUpHorizontalFlipDeg => 20;

        public override double SpeedMultiplier => 1;

        public override double WorldScale => 0.15;

        protected override string SoundsFolderPath => "Skeletons/Human";

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            // human legs collider
            var radius = 0.2;

            physicsBody.AddShapeCircle(
                radius / 2,
                center: (-radius / 2, 0));

            physicsBody.AddShapeCircle(
                radius / 2,
                center: (radius / 2, 0));

            radius *= 0.9;
            physicsBody.AddShapeRectangle(
                size: (radius, radius),
                offset: (-radius / 2, -radius / 2));

            // melee hitbox
            physicsBody.AddShapeRectangle(
                size: (0.6, 0.7),
                offset: (-0.3, 0.25),
                group: CollisionGroups.HitboxMelee);

            // ranged hitbox
            physicsBody.AddShapeRectangle(
                size: (0.5, 1.4),
                offset: (-0.25, 0),
                group: CollisionGroups.HitboxRanged);
        }

        public override void OnSkeletonCreated(IComponentSkeleton skeleton)
        {
            base.OnSkeletonCreated(skeleton);

            skeleton.SetMixDuration("Torch", 0.3f);
        }
    }
}