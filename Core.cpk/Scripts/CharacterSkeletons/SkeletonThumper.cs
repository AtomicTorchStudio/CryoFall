namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonThumper : ProtoCharacterSkeletonAnimal
    {
        public const float Scale = 1.25f;

        public override double DefaultMoveSpeed => 2.2 * Scale;

        public override Vector2D IconOffset => (75, -55);

        public override double IconScale => 0.375;

        public override SkeletonResource SkeletonResourceBack { get; }
            = new("Thumper/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("Thumper/Front");

        public override double WorldScale => 0.45 * Scale;

        protected override string SoundsFolderPath => "Skeletons/Thumper";

        protected override double VolumeFootsteps => 1.0;

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
            shadowRenderer.PositionOffset = (0, -0.02 * scaleMultiplier);
            shadowRenderer.Scale = 2.2 * scaleMultiplier * Scale;
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            // TODO: it should be not pushable (has a high mass) but also should not stuck in obstacles
            //physicsBody.IsNotPushable = true;

            physicsBody
                .AddShapeCircle(radius: 0.6,
                                center: (-0.1, 0.15))
                .AddShapeCircle(radius: 0.6,
                                center: (0.1, 0.15))
                .AddShapeCircle(radius: 0.7,
                                center: (0, 0.25),
                                group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.7,
                                center: (0, 0.25 + 0.3),
                                group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.7,
                                center: (0, 0.25 + 0.3 + 0.3),
                                group: CollisionGroups.HitboxMelee)
                // the size is carefully adjusted to ensure no damage through a horizontal wall when Thumper is above it
                .AddShapeCircle(radius: 0.65,
                                center: (0, 0.25),
                                group: CollisionGroups.HitboxRanged)
                .AddShapeCircle(radius: 0.65,
                                center: (0, 0.25 + 0.3),
                                group: CollisionGroups.HitboxRanged)
                .AddShapeCircle(radius: 0.65,
                                center: (0, 0.25 + 0.3 + 0.3),
                                group: CollisionGroups.HitboxRanged);
        }

        public override void GetCurrentAnimationSetting(
            ICharacter character,
            CharacterMoveModes moveModes,
            double angle,
            ViewOrientation lastViewOrientation,
            out string starterAnimationName,
            out string currentAnimationName,
            out DrawMode currentDrawMode,
            out float aimCoef,
            out ViewOrientation viewOrientation,
            out bool isIdle)
        {
            base.GetCurrentAnimationSetting(character,
                                            moveModes,
                                            angle,
                                            lastViewOrientation,
                                            out starterAnimationName,
                                            out currentAnimationName,
                                            out currentDrawMode,
                                            out aimCoef,
                                            out viewOrientation,
                                            out isIdle);

            var aiState = MobThumper.GetPublicState(character).AiState;
            switch (aiState)
            {
                case MobThumper.ThumperAiState.RushAttackPreparation:
                    var angleDeg = angle * MathConstants.RadToDeg;
                    if (angleDeg < 60                       // looking right
                        || angleDeg > 300                   // looking right
                        || angleDeg > 120 && angleDeg < 240 // looking left
                        )
                    {
                        currentAnimationName = "RushRunSidePrepare";
                    }
                    else
                    {
                        currentAnimationName = viewOrientation.IsUp ? "RushRunUpPrepare" : "RushRunDownPrepare";
                    }

                    //Logger.Dev($"Angle deg: {angleDeg} - animation name: {currentAnimationName}");
                    break;

                case MobThumper.ThumperAiState.RushAttack:
                    currentAnimationName = currentAnimationName.Replace("Run", "RushRun");
                    starterAnimationName = starterAnimationName?.Replace("Run", "RushRun");
                    break;
            }
        }

        public override void OnSkeletonCreated(IComponentSkeleton skeleton)
        {
            base.OnSkeletonCreated(skeleton);
            skeleton.DrawOrderOffsetY = -0.2;

            skeleton.AnimationEvent += SkeletonOnAnimationEventFootstep;

            // disable mix for these movement animations
            DisableMoveMix("RunUp");
            DisableMoveMix("RunDown");
            DisableMoveMix("RunSide");
            DisableMoveMix("RunSideBackward");
            DisableMoveMix("RushRunUp");
            DisableMoveMix("RushRunDown");
            DisableMoveMix("RushRunSide");
            DisableMoveMix("RushRunSideBackward");

            var verticalSpeedMultiplier = 1.1f;
            skeleton.SetAnimationDefaultSpeed("RunUp",            verticalSpeedMultiplier);
            skeleton.SetAnimationDefaultSpeed("RunUpStart",       verticalSpeedMultiplier);
            skeleton.SetAnimationDefaultSpeed("RunDown",          verticalSpeedMultiplier);
            skeleton.SetAnimationDefaultSpeed("RunDownStart",     verticalSpeedMultiplier);
            skeleton.SetAnimationDefaultSpeed("RushRunUp",        verticalSpeedMultiplier);
            skeleton.SetAnimationDefaultSpeed("RushRunUpStart",   verticalSpeedMultiplier);
            skeleton.SetAnimationDefaultSpeed("RushRunDown",      verticalSpeedMultiplier);
            skeleton.SetAnimationDefaultSpeed("RushRunDownStart", verticalSpeedMultiplier);

            void DisableMoveMix(string primaryName)
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

        protected override ReadOnlySoundPreset<GroundSoundMaterial> PrepareSoundPresetFootsteps()
        {
            var soundsFolderPath = this.SoundsFolderPath + "/Footsteps/";
            var preset = new SoundPreset<GroundSoundMaterial>(
                customDistance: (5, 20),
                customDistance3DSpread: (3, 12));
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