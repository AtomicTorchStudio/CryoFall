namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonMechSkipper : ProtoCharacterSkeleton
    {
        public const double MeleeHitboxHeight = 0.7;

        public const double MeleeHitboxOffset = 0.25;

        public const double RangedHitboxHeight = 1.4;

        public const double RangedHitboxOffset = 0;

        public override double DefaultMoveSpeed => 1.5;

        public override bool HasMoveStartAnimations => true;

        public override Vector2D InventoryOffset => (55, 55);

        public override double InventoryScale => 0.6;

        public override float OrientationDownExtraAngle => 35;

        public override float OrientationThresholdDownHorizontalFlipDeg => 25;

        public override float OrientationThresholdDownToUpFlipDeg => 45;

        public override float OrientationThresholdUpHorizontalFlipDeg => 20;

        public override SkeletonResource SkeletonResourceBack { get; }
            = new SkeletonResource("Mech/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new SkeletonResource("Mech/Front");

        public override string SlotNameItemInHand => "TurretLeft";

        public override double SpeedMultiplier => 1;

        public override double WorldScale => 0.15;

        // TODO: use proper sounds folder
        //protected override string SoundsFolderPath => "Skeletons/Mech";
        protected override string SoundsFolderPath => "Skeletons/Human";

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.1 * scaleMultiplier);
            shadowRenderer.Scale = 1.7 * scaleMultiplier;
            shadowRenderer.Color = Color.FromArgb(0x77, 0x00, 0x00, 0x00);
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            // mech legs collider
            var radius = 0.5;

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
                size: (0.6, MeleeHitboxHeight),
                offset: (-0.3, MeleeHitboxOffset),
                group: CollisionGroups.HitboxMelee);

            // ranged hitbox
            physicsBody.AddShapeRectangle(
                size: (0.5, RangedHitboxHeight),
                offset: (-0.25, RangedHitboxOffset),
                group: CollisionGroups.HitboxRanged);
        }

        public override void OnSkeletonCreated(IComponentSkeleton skeleton)
        {
            base.OnSkeletonCreated(skeleton);

            // little offset to ensure mech can properly behind a grass
            // cannot make it further without making it to z-fight with a player character
            skeleton.DrawOrderOffsetY = -0.3;

            skeleton.SetDefaultMixDuration(0.3f);

            // disable mix for these animations
            DisableMix("RunUp");
            DisableMix("RunDown");
            DisableMix("RunSide");
            DisableMix("RunSideBackward");

            var verticalSpeedMultiplier = 1.1f; // 1.25f;
            skeleton.SetAnimationDefaultSpeed("RunUp",        verticalSpeedMultiplier);
            skeleton.SetAnimationDefaultSpeed("RunUpStart",   verticalSpeedMultiplier);
            skeleton.SetAnimationDefaultSpeed("RunDown",      verticalSpeedMultiplier);
            skeleton.SetAnimationDefaultSpeed("RunDownStart", verticalSpeedMultiplier);

            void DisableMix(string primaryName)
            {
                var startName = primaryName + "Start";
                var startAbortName = startName + "Abort";
                var minMix = 0.05f;
                skeleton.SetMixDuration(startName,      minMix);
                skeleton.SetMixDuration(startAbortName, minMix);
                skeleton.SetMixDuration("Idle",         startName,      minMix);
                skeleton.SetMixDuration(startName,      primaryName,    minMix);
                skeleton.SetMixDuration(startName,      startAbortName, minMix);
                skeleton.SetMixDuration(startAbortName, "Idle",         minMix);

                skeleton.SetAnimationDefaultSpeed(startAbortName, 1.3f);
            }
        }
    }
}