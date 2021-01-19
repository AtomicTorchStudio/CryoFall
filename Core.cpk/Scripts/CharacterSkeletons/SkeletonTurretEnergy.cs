namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using static Systems.Physics.CollisionGroups;

    public class SkeletonTurretEnergy : ProtoSkeletonTurret
    {
        public override double DrawWorldPositionOffsetY => 0.17;

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody.IsNotPushable = true;
            physicsBody.AddShapeRectangle(size: (0.9, 0.6), offset: (0.05 - 0.5, 0))
                       .AddShapeRectangle(size: (0.8, 0.7), offset: (0.1 - 0.5, 0.4), group: HitboxMelee)
                       .AddShapeRectangle(size: (0.7, 0.5), offset: (0.15 - 0.5, 0.65), group: HitboxRanged);
        }

        public override void OnSkeletonCreated(IComponentSkeleton skeleton)
        {
            base.OnSkeletonCreated(skeleton);
            skeleton.DrawOrderOffsetY += 0.35;
        }
    }
}