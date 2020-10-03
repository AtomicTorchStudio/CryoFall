namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonPragmiumQueen : ProtoCharacterSkeletonAnimal
    {
        public override double DefaultMoveSpeed => 2.4;

        public override Vector2D IconOffset => (30, 30);

        public override double IconScale => 0.28;

        public override float OrientationDownExtraAngle => 10;

        public override float OrientationThresholdDownHorizontalFlipDeg => 10;

        public override float OrientationThresholdDownToUpFlipDeg => 15;

        public override float OrientationThresholdUpHorizontalFlipDeg => 5;

        public override SkeletonResource SkeletonResourceBack { get; }
            = new SkeletonResource("PragmiumQueen/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new SkeletonResource("PragmiumQueen/Front");

        public override double WorldScale => 0.75;

        protected override string SoundsFolderPath => "Skeletons/PragmiumQueen";

        protected override double VolumeFootsteps => 1.0;

        public override void ClientGetAimingOrientation(
            ICharacter character,
            double angleRad,
            ViewOrientation lastViewOrientation,
            out ViewOrientation viewOrientation,
            out float aimCoef)
        {
            if (character?.GetPublicState<CharacterMobPublicState>().SelectedItemWeaponProto
                    is ProtoItemMobWeaponNova)
            {
                // look down as this Spine2D skeleton don't have Nova attack orientation for back view
                viewOrientation = lastViewOrientation;
                viewOrientation.IsUp = false;
                aimCoef = 0;
                return;
            }

            base.ClientGetAimingOrientation(character,
                                            angleRad,
                                            lastViewOrientation,
                                            out viewOrientation,
                                            out aimCoef);
        }

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.675 * scaleMultiplier);
            shadowRenderer.Scale = 5.25 * scaleMultiplier;
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            physicsBody.IsNotPushable = true;

            physicsBody
                .AddShapeCircle(radius: 1.2,
                                center: (0 - 0.3375, 0.15))
                .AddShapeCircle(radius: 1.2,
                                center: (0 + 0.3375, 0.15))
                .AddShapeCircle(radius: 1.05,
                                center: (0, 0),
                                group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 1.05,
                                center: (0, 0.6),
                                group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 1.05,
                                center: (0, 0),
                                group: CollisionGroups.HitboxRanged)
                .AddShapeCircle(radius: 1.05,
                                center: (0, 0.75),
                                group: CollisionGroups.HitboxRanged)
                .AddShapeCircle(radius: 1.05,
                                center: (0, 1.5),
                                group: CollisionGroups.HitboxRanged);
        }

        public override void OnSkeletonCreated(IComponentSkeleton skeleton)
        {
            base.OnSkeletonCreated(skeleton);
            skeleton.DrawOrderOffsetY = -0.2;

            skeleton.AnimationEvent += SkeletonOnAnimationEventFootstep;

            // setup attack animations
            {
                var mixIn = 0.033333f;
                var mixOut = 0.15f;
                var mixInStatic = 0.15f;

                // fast mix-in into attacks, slower mix-out
                skeleton.SetMixDuration(null, "AttackRangedHorizontal",        mixIn,       mixOut);
                skeleton.SetMixDuration(null, "AttackRangedHorizontal_Static", mixInStatic, mixOut);
                skeleton.SetMixDuration(null, "AttackRangedVertical",          mixIn,       mixOut);
                skeleton.SetMixDuration(null, "AttackRangedVertical_Static",   mixInStatic, mixOut);

                // fast mix between attacks
                skeleton.SetMixDuration("AttackRangedHorizontal",        "AttackRangedVertical",        mixIn, mixIn);
                skeleton.SetMixDuration("AttackRangedHorizontal_Static", "AttackRangedVertical_Static", mixIn, mixIn);

                // fast attack animation
                skeleton.SetMixDuration(null, "AttackAOE", 0.0333f, 0.1f);
            }

            var sceneObject = skeleton.SceneObject;
            if (sceneObject.AttachedWorldObject is not null)
            {
                ClientLighting.CreateLightSourceSpot(
                    sceneObject,
                    color: LightColors.PragmiumLuminescenceSource,
                    size: 25,
                    spritePivotPoint: (0.5, 0.5),
                    positionOffset: (0, 0));
            }
        }

        protected override ReadOnlySoundPreset<CharacterSound> PrepareSoundPresetCharacter()
        {
            if (!Api.Shared.IsFolderExists(ContentPaths.Sounds + this.SoundsFolderPath))
            {
                throw new Exception("Sounds folder for " + this + " doesn't exist");
            }

            var preset = SoundPreset.CreateFromFolder<CharacterSound>(
                this.SoundsFolderPath + "/Character/",
                throwExceptionIfNoFilesFound: false,
                customDistance: (15, 45),
                customDistance3DSpread: (10, 35));
            //this.VerifySoundPreset(preset);
            return preset;
        }

        protected override ReadOnlySoundPreset<GroundSoundMaterial> PrepareSoundPresetFootsteps()
        {
            var soundsFolderPath = this.SoundsFolderPath + "/Footsteps/";
            var preset = new SoundPreset<GroundSoundMaterial>(
                customDistance: (15, 45),
                customDistance3DSpread: (10, 35));
            foreach (var enumValue in EnumExtensions.GetValues<GroundSoundMaterial>())
            {
                var soundFileName = GroundSoundMaterial.Solid;
                preset.Add(enumValue, soundsFolderPath + soundFileName);
            }

            var readOnlySoundPreset = preset.ToReadOnly();
            this.VerifySoundPreset(readOnlySoundPreset);
            return readOnlySoundPreset;
        }

        // Screen shakes on boss steps!
        private static void SkeletonOnAnimationEventFootstep(
            ICharacter character,
            IComponentSkeleton skeleton,
            SkeletonEventData e)
        {
            if (e.EventName != "Footstep")
            {
                return;
            }

            const float shakesDuration = 0.1f,
                        shakesDistanceMin = 0.1f,
                        shakesDistanceMax = 0.125f;
            ClientComponentCameraScreenShakes.AddRandomShakes(duration: shakesDuration,
                                                              worldDistanceMin: -shakesDistanceMin,
                                                              worldDistanceMax: shakesDistanceMax);
        }
    }
}