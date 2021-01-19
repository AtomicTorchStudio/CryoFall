namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoCharacterSkeletonAnimal : ProtoCharacterSkeleton
    {
        // cache singleton instance for human footsteps sound
        private static readonly Lazy<ReadOnlySoundPreset<GroundSoundMaterial>> HumanFootstepsSoundPreset
            = new(() =>
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

        private readonly Lazy<ITextureResource> lazyIcon;

        protected ProtoCharacterSkeletonAnimal()
        {
            this.lazyIcon = new Lazy<ITextureResource>(
                () => new ProceduralTexture(
                    "Creature icon " + this.ShortId,
                    request => this.GenerateIcon(request),
                    isTransparent: true,
                    isUseCache: true));
        }

        public sealed override bool HasMoveStartAnimations => false;

        public ITextureResource Icon => this.lazyIcon.Value;

        public virtual Vector2D IconOffset => Vector2D.Zero;

        public abstract double IconScale { get; }

        // This is a switch angle between front and back orientation.
        // We've made it larger for aggressive creature as they look (target) the human's melee center hitbox.
        // For peaceful creatures (like pangolin and snail) it should be overridden to a smaller value.
        public override float OrientationDownExtraAngle => 40;

        public override float OrientationThresholdDownHorizontalFlipDeg => 5;

        public override float OrientationThresholdDownToUpFlipDeg => 5;

        public override float OrientationThresholdUpHorizontalFlipDeg => 5;

        public override string SlotNameItemInHand => "Head";

        protected override double VolumeFootsteps => 0.5;

        public async Task<ITextureResource> GenerateIcon(
            ProceduralTextureRequest request,
            ushort textureWidth = 384,
            ushort textureHeight = 384,
            sbyte spriteQualityOffset = 0)
        {
            var protoSkeleton = this;

            var scale = protoSkeleton.IconScale * textureWidth / 256.0;
            var renderingTag = request.TextureName;

            var renderTarget = Api.Client.Rendering.CreateRenderTexture(renderingTag, textureWidth, textureHeight);
            var cameraObject = Api.Client.Scene.CreateSceneObject(renderingTag);
            var camera = Api.Client.Rendering.CreateCamera(cameraObject,
                                                           renderingTag: renderingTag,
                                                           drawOrder: -100);

            camera.RenderTarget = renderTarget;
            camera.ClearColor = Color.FromArgb(0, 0, 0, 0);
            camera.SetOrthographicProjection(textureWidth, textureHeight);

            var currentSkeleton = ClientCharacterEquipmentHelper.CreateCharacterSkeleton(
                cameraObject,
                protoSkeleton,
                worldScale: scale,
                spriteQualityOffset: spriteQualityOffset);
            currentSkeleton.PositionOffset = (textureWidth / 2.0 + this.IconOffset.X * scale,
                                              -textureHeight * 0.7 + this.IconOffset.Y * scale);
            currentSkeleton.RenderingTag = renderingTag;
            currentSkeleton.SetAnimationFrame(0, this.DefaultAnimationName, timePositionFraction: 0);

            await camera.DrawAsync();
            cameraObject.Destroy();

            request.ThrowIfCancelled();

            var generatedTexture = await renderTarget.SaveToTexture(
                                       isTransparent: true,
                                       qualityScaleCoef:
                                       Api.Client.Rendering.CalculateCurrentQualityScaleCoefWithOffset(
                                           spriteQualityOffset));

            currentSkeleton.Destroy();
            renderTarget.Dispose();
            request.ThrowIfCancelled();
            return generatedTexture;
        }

        public override void OnSkeletonCreated(IComponentSkeleton skeleton)
        {
            base.OnSkeletonCreated(skeleton);

            skeleton.SetDefaultMixDuration(0.3f);

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