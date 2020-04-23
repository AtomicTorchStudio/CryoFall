namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class SkeletonSpectator : ProtoCharacterSkeleton
    {
        public override double DefaultMoveSpeed => 1.5;

        public override bool HasMoveStartAnimations => false;

        public override float OrientationDownExtraAngle => 0;

        public override float OrientationThresholdDownHorizontalFlipDeg => 0;

        public override float OrientationThresholdDownToUpFlipDeg => 0;

        public override float OrientationThresholdUpHorizontalFlipDeg => 0;

        public override SkeletonResource SkeletonResourceBack => null;

        public override SkeletonResource SkeletonResourceFront => null;

        public override string SlotNameItemInHand => null;

        protected override string SoundsFolderPath => "Skeletons/Human";

        protected override double VolumeFootsteps => 0;

        public override void ClientSetupShadowRenderer(IComponentSpriteRenderer shadowRenderer, double scaleMultiplier)
        {
        }

        public override void CreatePhysics(IPhysicsBody physicsBody)
        {
            // no physics
        }

        protected override ReadOnlySoundPreset<GroundSoundMaterial> PrepareSoundPresetFootsteps()
        {
            return new SoundPreset<GroundSoundMaterial>();
        }
    }
}