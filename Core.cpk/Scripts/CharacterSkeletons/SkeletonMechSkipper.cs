namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class SkeletonMechSkipper : ProtoSkeletonMech
    {
        public override SkeletonResource SkeletonResourceBack { get; }
            = new SkeletonResource("MechSkipper/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new SkeletonResource("MechSkipper/Front");

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.1 * scaleMultiplier);
            shadowRenderer.Scale = 1.7 * scaleMultiplier;
            shadowRenderer.Color = Color.FromArgb(0x77, 0x00, 0x00, 0x00);
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
                center: (-radius / 2, 0));

            physicsBody.AddShapeCircle(
                radius / 2,
                center: (radius / 2, 0));

            physicsBody.AddShapeRectangle(
                size: (radius, radius),
                offset: (-radius / 2, -radius / 2));

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
    }
}