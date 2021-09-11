namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class SkeletonMechNemesis : ProtoSkeletonMech
    {
        public override SkeletonResource SkeletonResourceBack { get; }
            = new("MechNemesis/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("MechNemesis/Front");

        public override double WorldScale => 0.14;

        protected override float AnimationVerticalMovemementSpeedMultiplier => 1.25f;

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.01 * scaleMultiplier);
            shadowRenderer.Scale = (2.1 * scaleMultiplier, 2.3 * scaleMultiplier);
            shadowRenderer.Color = Color.FromArgb(0x99, 0x00, 0x00, 0x00);
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            const double radius = 0.5, // mech legs collider
                         meleeHitboxHeight = 0.7,
                         meleeHitboxOffset = 0.25,
                         rangedHitboxHeight = 1.4,
                         rangedHitboxOffset = 0;

            physicsBody.AddShapeCircle(
                radius / 2,
                center: (-radius / 2, 0),
                CollisionGroups.CharacterOrVehicle);

            physicsBody.AddShapeCircle(
                radius / 2,
                center: (radius / 2, 0),
                CollisionGroups.CharacterOrVehicle);

            physicsBody.AddShapeRectangle(
                size: (radius, radius),
                offset: (-radius / 2, -radius / 2),
                CollisionGroups.CharacterOrVehicle);

            // melee hitbox
            physicsBody.AddShapeRectangle(
                size: (0.8, meleeHitboxHeight),
                offset: (-0.4, meleeHitboxOffset),
                group: CollisionGroups.HitboxMelee);

            // ranged hitbox
            physicsBody.AddShapeRectangle(
                size: (0.8, rangedHitboxHeight),
                offset: (-0.4, rangedHitboxOffset),
                group: CollisionGroups.HitboxRanged);
        }

        public override void OnSkeletonCreated(IComponentSkeleton skeleton)
        {
            base.OnSkeletonCreated(skeleton);
            skeleton.PositionOffset = (0, -0.14);
            skeleton.DrawOrderOffsetY += 0.4;
        }
    }
}