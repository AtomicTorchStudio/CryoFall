namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public abstract class ProtoCharacterSkeletonHuman : ProtoCharacterSkeleton
    {
        public const double LegsColliderRadius = 0.2;

        public const double MeleeHitboxHeight = 0.7;

        public const double MeleeHitboxOffset = 0.25;

        public const double RangedHitboxHeight = 1.4;

        public const double RangedHitboxOffset = 0;

        public override double DefaultMoveSpeed => 2.25;

        public override bool HasMoveStartAnimations => true;

        public override float OrientationDownExtraAngle => 35;

        public override float OrientationThresholdDownHorizontalFlipDeg => 25;

        public override float OrientationThresholdDownToUpFlipDeg => 45;

        public override float OrientationThresholdUpHorizontalFlipDeg => 20;

        public override string SlotNameItemInHand => "Weapon";

        public override double SpeedMultiplier => 1;

        public override double WorldScale => 0.15;

        protected override string SoundsFolderPath => "Skeletons/Human";

        public override void ClientResetItemInHand(IComponentSkeleton skeletonRenderer)
        {
            skeletonRenderer.SetAttachmentSprite(this.SlotNameItemInHand,
                                                 attachmentName: "WeaponMelee",
                                                 textureResource: null);
            skeletonRenderer.SetAttachmentSprite(this.SlotNameItemInHand,
                                                 attachmentName: "WeaponRifle",
                                                 textureResource: null);
            skeletonRenderer.SetAttachment(this.SlotNameItemInHand, attachmentName: null);
        }

        public override void ClientSetupItemInHand(
            IComponentSkeleton skeletonRenderer,
            string attachmentName,
            TextureResource textureResource)
        {
            skeletonRenderer.SetAttachmentSprite(this.SlotNameItemInHand, attachmentName, textureResource);
            skeletonRenderer.SetAttachment(this.SlotNameItemInHand, attachmentName);

            // set left hand to use special sprite (representing holding of the item in hand)
            skeletonRenderer.SetAttachment("HandLeft",          "HandLeft2");
            skeletonRenderer.SetAttachment("HandLeftEquipment", "HandLeft2Equipment");
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            const double radius = LegsColliderRadius;
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

            skeleton.RemoveAttachmentAnimationsForSlot("Weapon");

            skeleton.SetDefaultMixDuration(0.3f);

            skeleton.SetMixDuration("Torch",  0.25f);
            skeleton.SetMixDuration("Torch2", 0.15f);

            skeleton.SetMixDuration("Fishing_In", 0.1f);
            skeleton.SetMixDuration("Fishing_Out", 0.1f);

            // instant transition from death to idle animation
            skeleton.SetMixDuration("Idle", "Death", secondsMixAB: 0.2f, secondsMixBA: 0);

            // setup attack animations
            {
                var mixIn = 0.033333f;
                var mixInStatic = 0.15f;
                var mixOut = 0.15f;
                // fast mix-in into attacks, slower mix-out
                skeleton.SetMixDuration(null, "AttackMeleeHorizontal",        mixIn,       mixOut);
                skeleton.SetMixDuration(null, "AttackMeleeHorizontal_Static", mixInStatic, mixOut);
                skeleton.SetMixDuration(null, "AttackMeleeVertical",          mixIn,       mixOut);
                skeleton.SetMixDuration(null, "AttackMeleeVertical_Static",   mixInStatic, mixOut);

                // fast mix between attacks
                skeleton.SetMixDuration("AttackMeleeHorizontal",        "AttackMeleeVertical",        mixIn, mixIn);
                skeleton.SetMixDuration("AttackMeleeHorizontal_Static", "AttackMeleeVertical_Static", mixIn, mixIn);
            }

            // disable mix for these movement animations
            DisableMoveMix("RunUp");
            DisableMoveMix("RunDown");
            DisableMoveMix("RunSide");
            DisableMoveMix("RunSideBackward");

            var verticalSpeedMultiplier = 1.1f;
            skeleton.SetAnimationDefaultSpeed("RunUp",        verticalSpeedMultiplier);
            skeleton.SetAnimationDefaultSpeed("RunUpStart",   verticalSpeedMultiplier);
            skeleton.SetAnimationDefaultSpeed("RunDown",      verticalSpeedMultiplier);
            skeleton.SetAnimationDefaultSpeed("RunDownStart", verticalSpeedMultiplier);

            void DisableMoveMix(string primaryName)
            {
                var startName = primaryName + "Start";
                var startAbortName = startName + "Abort";
                var minMix = 0.05f;
                skeleton.SetMixDuration(startName,      minMix);
                skeleton.SetMixDuration(startAbortName, minMix);
                skeleton.SetMixDuration("Idle",         startName,      minMix);
                // in theory there should be no mixing but we have to apply some
                // as end state of the movement start animation is not always perfectly matching
                // the start state of the movement animation 
                skeleton.SetMixDuration(startName,      primaryName,    minMix);
                skeleton.SetMixDuration(startName,      startAbortName, minMix);
                skeleton.SetMixDuration(startAbortName, "Idle",         minMix);

                skeleton.SetAnimationDefaultSpeed(startAbortName, 1.3f);
            }
        }
    }
}