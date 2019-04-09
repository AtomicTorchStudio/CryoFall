namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
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
                        // use Solid sound for all ground materials except Vegetation
                        var soundFileName = enumValue == GroundSoundMaterial.Vegetation
                                                ? GroundSoundMaterial.Vegetation
                                                : GroundSoundMaterial.Solid;

                        preset.Add(enumValue, localSoundsFolderPath + soundFileName);
                    }

                    var readOnlySoundPreset = preset.ToReadOnly();
                    //this.VerifySoundPreset(readOnlySoundPreset);
                    return readOnlySoundPreset;
                });

        public sealed override bool HasMoveStartAnimations => false;

        public override float OrientationDownExtraAngle => 15;

        public override float OrientationThresholdDownHorizontalFlipDeg => 5;

        public override float OrientationThresholdDownToUpFlipDeg => 5;

        public override float OrientationThresholdUpHorizontalFlipDeg => 5;

        protected override double VolumeFootsteps => 0.5;

        protected override ReadOnlySoundPreset<GroundSoundMaterial> PrepareSoundPresetFootsteps()
        {
            // reuse human sounds
            return HumanFootstepsSoundPreset.Value;
        }
    }
}