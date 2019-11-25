namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Extensions;

    public abstract class ProtoCharacterSkeletonAnimal : ProtoCharacterSkeleton
    {
        // cache singleton instance for human footsteps sound
        private static readonly Lazy<ReadOnlySoundPreset<GroundSoundMaterial>> HumanFootstepsSoundPreset
            = new Lazy<ReadOnlySoundPreset<GroundSoundMaterial>>(
                () =>
                {
                    const string localSoundsFolderPath = "Skeletons/Human/Footsteps/";

                    var preset = new SoundPreset<GroundSoundMaterial>();
                    foreach (var enumValue in EnumExtensions.GetValues<GroundSoundMaterial>())
                    {
                        var soundFileName = GroundSoundMaterial.Solid;

                        // use Solid sound for all ground materials except Vegetation and Mud
                        switch (enumValue)
                        {
                            case GroundSoundMaterial.Vegetation:
                            case GroundSoundMaterial.Mud:
                                soundFileName = enumValue;
                                break;
                        }

                        preset.Add(enumValue, localSoundsFolderPath + soundFileName);
                    }

                    var readOnlySoundPreset = preset.ToReadOnly();
                    //this.VerifySoundPreset(readOnlySoundPreset);
                    return readOnlySoundPreset;
                });

        public sealed override bool HasMoveStartAnimations => false;

        // This is a switch angle between front and back orientation.
        // We've made it larger for aggressive creature as they look (target) the human's melee center hitbox.
        // For peaceful creatures (like pangolin and snail) it should be overridden to a smaller value.
        public override float OrientationDownExtraAngle => 40;

        public override float OrientationThresholdDownHorizontalFlipDeg => 5;

        public override float OrientationThresholdDownToUpFlipDeg => 5;

        public override float OrientationThresholdUpHorizontalFlipDeg => 5;

        protected override double VolumeFootsteps => 0.5;

        public override void OnSkeletonCreated(IComponentSkeleton skeleton)
        {
            base.OnSkeletonCreated(skeleton);

            skeleton.SetDefaultMixDuration(0.3f);

            // setup attack animations
            {
                var mixIn = 0.033333f;
                var mixInStatic = 0.15f;
                var mixOut = 0.15f;
                skeleton.SetMixDuration(null, "AttackMeleeHorizontal",        mixIn,       mixOut);
                skeleton.SetMixDuration(null, "AttackMeleeHorizontal_Static", mixInStatic, mixOut);
                skeleton.SetMixDuration(null, "AttackMeleeVertical",          mixIn,       mixOut);
                skeleton.SetMixDuration(null, "AttackMeleeVertical_Static",   mixInStatic, mixOut);
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
                skeleton.SetMixDuration(startName,      primaryName,    minMix);
                skeleton.SetMixDuration(startName,      startAbortName, minMix);
                skeleton.SetMixDuration(startAbortName, "Idle",         minMix);

                skeleton.SetAnimationDefaultSpeed(startAbortName, 1.3f);
            }
        }

        protected override ReadOnlySoundPreset<GroundSoundMaterial> PrepareSoundPresetFootsteps()
        {
            // reuse human sounds
            return HumanFootstepsSoundPreset.Value;
        }
    }
}